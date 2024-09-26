using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Discord.Interactions;

using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Models;
using TwitchLib.Communication.Clients;

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

using Source.Handler;
using Source.DataClasses;
using Debug.Update;
using System.Timers;
using Debug.Prefix;
using System.Runtime.Intrinsics.X86;


namespace Debug
{
    public class Program
    {
        public static DiscordSocketClient discordClient;
        public static CommandService commands;
        public static InteractionService interactions;

        public static TwitchClient twitchClient;
        public static IServiceProvider services;

        private static Task Main(string[] args) => new Program().MainAsync();

        public async Task MainAsync()
        {
            Source.Program.Setup();
            
            #region discord
            discordClient = new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.All,
                LogGatewayIntentWarnings = false,
                AlwaysDownloadUsers = true,
                LogLevel = LogSeverity.Info
            });

            commands = new CommandService(new CommandServiceConfig
            {
                LogLevel = LogSeverity.Debug,
                DefaultRunMode = Discord.Commands.RunMode.Async
            });

            interactions = new InteractionService(discordClient.Rest, new InteractionServiceConfig
            {
                LogLevel = LogSeverity.Debug,
                DefaultRunMode = Discord.Interactions.RunMode.Async
            });

            services = new ServiceCollection()
                .AddSingleton(discordClient)
                .AddSingleton(commands)
                .AddSingleton(interactions)
                .BuildServiceProvider();

            await commands.AddModulesAsync(Assembly.GetEntryAssembly(), services);
            await interactions.AddModulesAsync(Assembly.GetEntryAssembly(), services);


            discordClient.Log += async (LogMessage msg) => { if (msg.Message != "Ready") Log.Print(msg.Message, Logtype.Info, "Discord"); };
            discordClient.Ready += async () => Log.Print("Ready", Logtype.Info, "Discord"); // U know why

            commands.Log += async (LogMessage msg) => Log.Print(msg.Message, Logtype.Info, "Discord");
            interactions.Log += async (LogMessage msg) => Log.Print(msg.Message, Logtype.Info, "Discord");

            PrefixHandler.Setup(discordClient, commands, services);
            InteractionHandler.Setup(discordClient, interactions, services);

            discordClient.MessageReceived += PrefixHandler.HandleCommandAsync;
            discordClient.InteractionCreated += InteractionHandler.HandleInteraction;

            discordClient.Ready += async () => await interactions.RegisterCommandsGloballyAsync();
            discordClient.Ready += async () => await interactions.RegisterCommandsToGuildAsync(ulong.Parse(Source.Program.config["discord:guild"]));


            interactions.SlashCommandExecuted += InteractionHandler.SlashCommandExecuted;
            interactions.ContextCommandExecuted += InteractionHandler.ContextCommandExecuted;
            interactions.ComponentCommandExecuted += InteractionHandler.ComponentCommandExecuted;


            commands.CommandExecuted += async (Optional<CommandInfo> arg1, ICommandContext arg2, Discord.Commands.IResult arg3) =>
            {
                if (!arg3.IsSuccess && arg3.Error != CommandError.UnknownCommand)
                {
                    var embed = new EmbedBuilder()
                    {
                        Color = Discord.Color.Red,
                        Fields =
                        {
                            new EmbedFieldBuilder()
                            {
                                Name = $"Error: {arg3.Error}",
                                Value = arg3.ErrorReason
                            }
                        }
                    };
                    await arg2.Message.ReplyAsync(embed: embed.Build(), allowedMentions: AllowedMentions.None);
                    Log.Print(arg3.ErrorReason, Logtype.Error, "Error");
                }
            };

            interactions.InteractionExecuted += async (ICommandInfo arg1, IInteractionContext arg2, Discord.Interactions.IResult arg3) =>
            {
                if (!arg3.IsSuccess && arg3.Error != InteractionCommandError.UnknownCommand)
                {
                    var embed = new EmbedBuilder()
                    {
                        Color = Discord.Color.Red,
                        Fields =
                        {
                            new EmbedFieldBuilder()
                            {
                                Name = $"Error: {arg3.Error}",
                                Value = arg3.ErrorReason
                            }
                        }
                    };
                    await arg2.Interaction.RespondAsync(ephemeral: true, embed: embed.Build(), allowedMentions: AllowedMentions.None);
                    Log.Print(arg3.ErrorReason, Logtype.Error, "Error");
                }
            };

            #endregion

            #region twitch
            var clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = 750,
                ThrottlingPeriod = TimeSpan.FromSeconds(30),
            };
            WebSocketClient customClient = new WebSocketClient(clientOptions);
            twitchClient = new TwitchClient(customClient);


            twitchClient.RemoveChatCommandIdentifier('!');
            twitchClient.AddChatCommandIdentifier(Source.Program.config["prefix"][0]);
            twitchClient.OnLog += (object? sender, OnLogArgs e) => Log.Print(e.Data.ToString(), Logtype.Info, "Twitch");

            twitchClient.OnConnected += (object? sender, OnConnectedArgs e) => twitchClient.JoinChannel(Source.Program.config["twitch:channel"]);
            //twitchClient.OnConnected += (object? sender, OnConnectedArgs e) => twitchClient.JoinChannel("forestguyvt");
            #endregion

            #region initalize
            await discordClient.LoginAsync(
                TokenType.Bot,
                Source.Program.config["discord:token"]
            );
            await discordClient.StartAsync();


            twitchClient.Initialize(
                new ConnectionCredentials(
                    Source.Program.config["twitch:username"],
                    Source.Program.config["twitch:token"]
                )
            );
            //twitchClient.Connect();
            #endregion

            discordClient.MessageReceived += AfkCommands.AfkResponses;
            discordClient.Ready += async () =>
            {
                await Source.Program.LoadEmoji(
                    emotes: await discordClient.Rest.GetApplicationEmotesAsync()
                );

                DateTime now = DateTime.Now;
                DateTime nextHour = now.AddHours(1).Date.AddHours(now.Hour + 1);
                TimeSpan timeUntilNextHour = nextHour - now;

                Log.Print($"Time until update: {timeUntilNextHour.Minutes}m {timeUntilNextHour.Seconds}s");
                System.Timers.Timer initialTimer = new System.Timers.Timer((int)timeUntilNextHour.TotalMilliseconds);
                initialTimer.Elapsed += async (sender, e) =>
                {
                    OnTimedEvent.Update();

                    System.Timers.Timer hourlyTimer = new System.Timers.Timer(3600000); // 3600000 ms = 1 hour
                    hourlyTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent.Start);
                    hourlyTimer.AutoReset = true;
                    hourlyTimer.Enabled = true;

                    initialTimer.Stop();
                    initialTimer.Dispose();
                };
                initialTimer.AutoReset = false;
                initialTimer.Enabled = true;
            };

            await Task.Delay(-1);
        }
    }
}
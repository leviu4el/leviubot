using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.Webhook;
using Discord.WebSocket;
using leviubot.Handler;
using leviubot.Logger;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using static leviubot.Commands.Interactions.VerificationModule;

namespace leviubot;

public class Program
{
    private static Task Main(string[] args) => new Program().MainAsync();
    private async Task MainAsync()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddYamlFile("config.yml")
            .Build();

        using IHost host = Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) => services
                .AddSingleton(config)
                .AddTransient<ConsoleLogger>()

                .AddSingleton(x => new DiscordSocketClient(new DiscordSocketConfig
                {
                    GatewayIntents = GatewayIntents.All,
                    LogGatewayIntentWarnings = false,
                    AlwaysDownloadUsers = true,
                    LogLevel = LogSeverity.Info
                }))

                .AddSingleton(x => new CommandService(new CommandServiceConfig
                {
                    LogLevel = LogSeverity.Debug,
                    DefaultRunMode = Discord.Commands.RunMode.Async
                }))
                .AddSingleton<PrefixHandler>()

                .AddSingleton(x => new InteractionService(config: new InteractionServiceConfig
                {
                    LogLevel = LogSeverity.Debug,
                    DefaultRunMode = Discord.Interactions.RunMode.Async
                },
                    discordProvider: x.GetRequiredService<DiscordSocketClient>()
                ))
                .AddSingleton<InteractionHandler>()

            ).Build();

        await RunAsync(host);
    }
    private async Task RunAsync(IHost host)
    {
        using IServiceScope serviceScope = host.Services.CreateScope();
        IServiceProvider provider = serviceScope.ServiceProvider;

        var client = provider.GetRequiredService<DiscordSocketClient>();
        var config = provider.GetRequiredService<IConfigurationRoot>();
        var commands = provider.GetRequiredService<CommandService>();
        var interactions = provider.GetRequiredService<InteractionService>();

        await provider.GetRequiredService<PrefixHandler>().InitializeAsync();
        await provider.GetRequiredService<InteractionHandler>().InitializeAsync();

        var logger = provider.GetRequiredService<ConsoleLogger>();

        client.Log += async msg => await logger.PrintAsync(msg.Message, Logtype.Info, "Discord");
        commands.Log += async msg => await logger.PrintAsync(msg.Message, Logtype.Info, "Discord");
        interactions.Log += async msg => await logger.PrintAsync(msg.Message, Logtype.Info, "Discord");
        

        client.Ready += async () =>
        {
            //await interactions.RegisterCommandsGloballyAsync();
            await interactions.RegisterCommandsToGuildAsync(ulong.Parse(config["discord:guild"]!));
            //await interactions.RegisterCommandsToGuildAsync(1136278281319096522); 
            try
            {
                interactions.AddModalInfo<AltVerificationModal>();
            }
            catch (Exception)
            {
                Console.WriteLine("add modal error");
            }
        };

        await client.LoginAsync(TokenType.Bot, config["discord:token"]);
        await client.StartAsync();

        await Task.Delay(-1);
    }


}

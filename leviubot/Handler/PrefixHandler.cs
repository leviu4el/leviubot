using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using leviubot.Logger;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace leviubot.Handler
{
    public class PrefixHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IServiceProvider _services;
        private readonly IConfigurationRoot _config;

        public PrefixHandler(DiscordSocketClient client, CommandService commands, IServiceProvider services, IConfigurationRoot config)
        {
            _client = client;
            _commands = commands;
            _services = services;
            _config = config;
        }

        public async Task InitializeAsync()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            _client.MessageReceived += HandleCommandAsync;
            _commands.CommandExecuted += CommandExecuted;
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            int argPos = 0;
            var message = messageParam as SocketUserMessage;

            if (message == null) return;
            if (message.Author.IsBot || message.Author.IsWebhook) return;

            var context = new SocketCommandContext(_client, message);

            await _commands.ExecuteAsync(
                context: context,
                argPos: argPos,
                services: _services
            );
        }
        
        private async Task CommandExecuted(Optional<CommandInfo> arg1, ICommandContext arg2, IResult arg3)
        {
            if (!arg3.IsSuccess && arg3.Error != CommandError.UnknownCommand)
            {
                var embed = new EmbedBuilder
                {
                    Color = Color.Red,
                    Fields =
                    {
                        new EmbedFieldBuilder
                        {
                            Name = $"Error: {arg3.Error}",
                            Value = arg3.ErrorReason
                        }
                    }
                };
                await arg2.Message.ReplyAsync(embed: embed.Build(), allowedMentions: AllowedMentions.None);
                await _services.GetRequiredService<ConsoleLogger>().PrintAsync(arg3.ErrorReason, Logtype.Error, "Error");
            }
        }
    }
}

using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace leviubot.Handler
{
    public class PrefixHandler
    {
        private static DiscordSocketClient _discordClient;
        private static CommandService _commands;
        private static IServiceProvider _services;

        public static void Setup(DiscordSocketClient discordClient, CommandService commands, IServiceProvider services)
        {
            _discordClient = discordClient;
            _commands = commands;
            _services = services;
        }
        public static async Task HandleCommandAsync(SocketMessage messageParam)
        {
            int argPos = 0;
            var message = messageParam as SocketUserMessage;

            if (message == null) return;
            if (message.Author.IsBot || message.Author.IsWebhook) return;
            if (!message.HasCharPrefix(Source.Program.config["prefix"][0], ref argPos)) return;

            var context = new SocketCommandContext(_discordClient, message);

            await _commands.ExecuteAsync(
                context: context,
                argPos: argPos,
                services: _services
            );
        }
    }
}

using Discord.Interactions;
using Discord.WebSocket;
using Discord;

namespace Source.Handler
{
    public class InteractionHandler
    {
        private static DiscordSocketClient _discordClient;
        private static InteractionService _interactions;
        private static IServiceProvider _services;

        public static void Setup(DiscordSocketClient discordClient, InteractionService interactions, IServiceProvider services)
        {
            _discordClient = discordClient;
            _interactions = interactions;
            _services = services;
        }

        public static Task ComponentCommandExecuted(ComponentCommandInfo arg1, IInteractionContext arg2, IResult arg3) { return Task.CompletedTask; }
        public static Task ContextCommandExecuted(ContextCommandInfo arg1, IInteractionContext arg2, IResult arg3) { return Task.CompletedTask; }
        public static Task SlashCommandExecuted(SlashCommandInfo arg1, IInteractionContext arg2, IResult arg3) { return Task.CompletedTask; }

        public static async Task HandleInteraction(SocketInteraction arg)
        {
            var ctx = new SocketInteractionContext(_discordClient, arg);
            await _interactions.ExecuteCommandAsync(ctx, _services);
        }
    }
}

using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using leviubot.Logger;
using Microsoft.Extensions.DependencyInjection;

namespace leviubot.Handler
{
    public class InteractionHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _interactions;
        private readonly IServiceProvider _services;

        public InteractionHandler(DiscordSocketClient client, InteractionService interactions, IServiceProvider services)
        {
            _client = client;
            _interactions = interactions;
            _services = services;
        }
        
        public async Task InitializeAsync()
        {
            await _interactions.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            _client.InteractionCreated += HandleInteraction;
            
            _interactions.SlashCommandExecuted += SlashCommandExecuted;
            _interactions.ContextCommandExecuted += ContextCommandExecuted;
            _interactions.ComponentCommandExecuted += ComponentCommandExecuted;

            _interactions.InteractionExecuted += InteractionExecuted;
        }

        public static Task ComponentCommandExecuted(ComponentCommandInfo arg1, IInteractionContext arg2, IResult arg3) { return Task.CompletedTask; }
        public static Task ContextCommandExecuted(ContextCommandInfo arg1, IInteractionContext arg2, IResult arg3) { return Task.CompletedTask; }
        public static Task SlashCommandExecuted(SlashCommandInfo arg1, IInteractionContext arg2, IResult arg3) { return Task.CompletedTask; }

        private async Task HandleInteraction(SocketInteraction arg)
        {
            var ctx = new SocketInteractionContext(_client, arg);
            await _interactions.ExecuteCommandAsync(ctx, _services);
        }

        private async Task InteractionExecuted(ICommandInfo _, IInteractionContext arg2, IResult arg3)
        {
            if (!arg3.IsSuccess && arg3.Error != InteractionCommandError.UnknownCommand)
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
                await arg2.Interaction.RespondAsync(ephemeral: true, embed: embed.Build(), allowedMentions: AllowedMentions.None);
                await _services.GetRequiredService<ConsoleLogger>().PrintAsync(arg3.ErrorReason, Logtype.Error, "Error");
            }
        }
    }
}

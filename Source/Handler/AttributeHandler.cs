using Discord.Commands;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace leviubot.Source.Handler
{
    public class AttributeHandler
    {

        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
        public class SlashRequireServerAttribute(ulong serverId) : Discord.Interactions.PreconditionAttribute
        {
            public override Task<Discord.Interactions.PreconditionResult> CheckRequirementsAsync(IInteractionContext context, Discord.Interactions.ICommandInfo cmdInfo, IServiceProvider services)
                => Task.FromResult(
                    context.Guild.Id == serverId 
                        ? Discord.Interactions.PreconditionResult.FromSuccess() 
                        : Discord.Interactions.PreconditionResult.FromError("You are not allowed to use this command.")
                    );
        }
    }
}

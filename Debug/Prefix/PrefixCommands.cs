using Debug.Update;
using Discord;
using Discord.Commands;
using Source.DataClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Source.DataClasses.Extensions;

namespace Debug.Prefix
{
    [Summary("Test")]
    public class PrefixCommands : ModuleBase<SocketCommandContext>
    {
        [Command("update")]
        [RequireOwner]
        public async Task test(params string[] parameters)
        {
            OnTimedEvent.Update();
        }
    }
}

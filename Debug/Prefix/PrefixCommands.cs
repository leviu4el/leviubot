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
        [Command("test")]
        public async Task test(string name = null, params string[] parameters)
        {
            
            TetrisUser User = new TetrisUser(name);
            User.GetUserInfo();

            //Extensions.Require(
            //    User.Info.Username,

            //    User.TetraLeague.CurrentSeason.apm,
            //    User.TetraLeague.CurrentSeason.pps,
            //    User.TetraLeague.CurrentSeason.vs
            //);
            await Context.Message.ReplyAsync(User.Info.Username);
        }
    }
}

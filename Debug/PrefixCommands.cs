using Discord;
using Discord.Commands;
using leviubot.DataClasses;
using Source.DataClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Source.DataClasses.Extensions;

namespace Debug.Prefix
{
    public class PrefixCommands : ModuleBase<SocketCommandContext>
    {
        [Command("test")]
        public async Task test(string name = null, params string[] parameters)
        {
            TetrisUser User = new TetrisUser(name, Context.User.Id);
            User.GetUserInfo();

            await Context.Message.ReplyAsync(User.Info.Username);
        }
    }
}

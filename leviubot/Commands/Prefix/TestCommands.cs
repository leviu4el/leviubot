using System.Globalization;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace leviubot.Commands.Prefix;

public class TestCommands : ModuleBase<SocketCommandContext>
{
    [Command("ping")]
    public async Task Ping(params string[] parameters)
    {
        await ReplyAsync("pong");
    }
}
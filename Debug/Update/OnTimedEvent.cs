using Discord.WebSocket;
using Discord;
using Source.DataClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Newtonsoft.Json;

namespace Debug.Update
{
    public class OnTimedEvent
    {
        private const char invisibleChar = ' '; // Zero Width Space

        public static async void Start(object source, ElapsedEventArgs e) => Update();
        public static async void Update()
        {
            var guild = Program.discordClient.GetGuild(ulong.Parse(Source.Program.config["leaderboard:guild"]));
            List<ulong> usersIds = guild.Users.Where(x => x.Roles.Any(x => x.Id == ulong.Parse(Source.Program.config["leaderboard:role"]))).Select(x => x.Id).ToList();
            List<TetrisUser> users = new List<TetrisUser>();
            foreach (var userid in usersIds)
            {
                var user = new TetrisUser(null, userid, spamthing: true);
                user.GetUserInfo();
                user.GetAll();

                users.Add(user);
            }
            Log.Print("Updating users");
            await Update40Lines(    users.Where(user => { try { Extensions.Require(user.FourtyLines.FinalTime); return true; } catch { return false; } }).ToList());
            await UpdateBlitz(      users.Where(user => { try { Extensions.Require(user.Blitz.Score); return true; } catch { return false; } }).ToList());
            await UpdateRating(     users.Where(user => { try { Extensions.Require(user.TetraLeague.CurrentSeason.Tr); return true; } catch { return false; } }).ToList());
            await UpdateAchivements(users);
            Log.Print("Updating finished");
        }

        public static async Task Update40Lines(List<TetrisUser> users)
        {
            var guild = Program.discordClient.GetGuild(ulong.Parse(Source.Program.config["leaderboard:guild"]));
            var ch = guild.GetTextChannel(ulong.Parse(Source.Program.config["leaderboard:40lines"]));

            var filteredUsers = users
                .OrderBy(x => x.FourtyLines.Rank)
                .ToList();

            var doneFilteredUsers = filteredUsers
            .Select(x => new
            {
                Username = x.Info.Username,
                Value = $"{(int)x.FourtyLines.FinalTime / 60000}:{(int)x.FourtyLines.FinalTime / 1000 % 60:00}:{(int)x.FourtyLines.FinalTime % 1000:000}",
            });

            List<string> strings = new List<string>();
            if (doneFilteredUsers.Count() > 0)
            {
                int maxUsernameLength = doneFilteredUsers.Max(u => u.Username.Length);
                int maxValueLength = doneFilteredUsers.Max(u => $"{u.Value}".Length);

                strings = doneFilteredUsers
                   .Select(u =>
                   $"`{u.Username}`" +
                   $"{u.Username.PadRight(maxUsernameLength, invisibleChar).Replace(u.Username, string.Empty)} " +
                   $"{$"`{u.Value}`".PadLeft(maxValueLength + 4, invisibleChar)} ").ToList();
            }

            List<EmbedBuilder> embeds = new List<EmbedBuilder>()
                .Concat(
                    new EmbedBuilder() { Color = Discord.Color.Green }
                    .AddStrings("Leaderboard", strings)
                )
                .ToList();
            embeds.ForEach(x => TryingSendEmbedToChannel(x, ch, embeds.IndexOf(x)));
        }
        public static async Task UpdateBlitz(List<TetrisUser> users)
        {
            var guild = Program.discordClient.GetGuild(ulong.Parse(Source.Program.config["leaderboard:guild"]));
            var ch = guild.GetTextChannel(ulong.Parse(Source.Program.config["leaderboard:blitz"]));


            var filteredUsers = users
                //.Where(x => x.Blitz.Rank != int.MinValue)
                .OrderBy(x => x.Blitz.Score)
                .Reverse()
                .ToList();

            var doneFilteredUsers = filteredUsers
            .Select(x => new
            {
                Username = x.Info.Username,
                Value = x.Blitz.Score,
            });

            List<string> strings = new List<string>();
            if (doneFilteredUsers.Count() > 0)
            {
                int maxUsernameLength = doneFilteredUsers.Max(u => u.Username.Length);
                int maxValueLength = doneFilteredUsers.Max(u => $"{u.Value}".Length);

                strings = doneFilteredUsers
                   .Select(u =>
                   $"`{u.Username}`" +
                   $"{u.Username.PadRight(maxUsernameLength, invisibleChar).Replace(u.Username, string.Empty)} " +
                   $"{$"`{u.Value}`".PadLeft(maxValueLength + 4, invisibleChar)} ").ToList();
            }

            List<EmbedBuilder> embeds = new List<EmbedBuilder>()
                .Concat(
                    new EmbedBuilder() { Color = Discord.Color.Green }
                    .AddStrings("Leaderboard", strings)
                )
                .ToList();
            embeds.ForEach(x => TryingSendEmbedToChannel(x, ch, embeds.IndexOf(x)));
        }
        public static async Task UpdateRating(List<TetrisUser> users) { }
        public static async Task UpdateAchivements(List<TetrisUser> users)
        {
            var guild = Program.discordClient.GetGuild(ulong.Parse(Source.Program.config["leaderboard:guild"]));
            var ch = guild.GetTextChannel(ulong.Parse(Source.Program.config["leaderboard:achivements"]));

            for (int id = 1; id < 50; id++)
            {
                if (id != 11 && id != 42)
                {
                    AchievementData achievementData = AchievementData.From(Source.Program.GetResult($"https://ch.tetr.io/api/achievements/{id}", spamthing: true).data);
                    var embed = new EmbedBuilder()
                    {
                        Color = Discord.Color.Green,
                        Fields =
                        {
                            new EmbedFieldBuilder()
                            {
                                Name = achievementData.name,
                                Value = achievementData.description
                            },
                        }
                    };

                    if (achievementData.cutoffs.Count() > 0)
                    {
                        int maxCutoffsValueLength = achievementData.cutoffs.Max(u => $"{u.Value}".Length);
                        embed.AddField(
                            new EmbedFieldBuilder()
                            {
                                Name = "Cutoffs",
                                Value = string.Join("\n", achievementData.cutoffs.Select(x => $"{Source.Program.ArEmoji[x.Key]} {$"`{x.Value}`".PadLeft(maxCutoffsValueLength + 2, invisibleChar)}")),
                                IsInline = true
                            }
                        );
                    }
                    if (achievementData.rankType == Achievement.rt.ZENITH)
                    {
                        var dick = new Dictionary<ArType, dynamic>()
                        {
                            { ArType.Diamond, "Floor 10" },
                            { ArType.Platinum, "Floor 9" },
                            { ArType.Gold, "Floor 7" },
                            { ArType.Silver, "Floor 5" },
                            { ArType.Bronze, "Floor 3" },
                        };
                        int maxZenithValueLength = dick.Max(u => $"{u.Value}".Length);
                        embed.AddField(
                            new EmbedFieldBuilder()
                            {
                                Name = "‎",
                                Value = string.Join("\n", dick.Select(x => $"{Source.Program.ArEmoji[x.Key]} {$"`{x.Value}`".PadLeft(maxZenithValueLength + 2, invisibleChar)}")),
                                IsInline = true
                            }
                        );
                    }

                    var filteredUsers = users
                        .Where(x => x.Achievements.First(a => a.Id == id).Rank != ArType.None)
                        .OrderBy(x => x.Achievements.First(a => a.Id == id).Pos)
                        .ToList();

                    var nonerankUsers = users
                        .Where(x => x.Achievements.First(a => a.Id == id).Rank == ArType.None && x.Achievements.First(a => a.Id == id).Unlocked)
                        .OrderBy(x => x.Achievements.First(a => a.Id == id).MainValue)
                        .ToList();
                    if (achievementData.valueType == Achievement.vt.TIME_INV || achievementData.valueType == Achievement.vt.NUMBER_INV)
                    {
                        nonerankUsers.Reverse();
                    }

                    //var lockedrankUsers = users
                    //    .Where(x => x.achievements.First(a => a.id == value).unlocked == false)
                    //    .ToList();

                    filteredUsers = filteredUsers.Concat(nonerankUsers).ToList(); //.Concat(lockedrankUsers)

                    var doneFilteredUsers = filteredUsers
                    .Select(x => new
                    {
                        Username = x.Info.Username,
                        Value = x.Achievements.First(a => a.Id == id).Value,
                        RankEmoji = Source.Program.ArEmoji[x.Achievements.First(a => a.Id == id).Rank]
                    })
                    .ToList();

                    List<string> strings = new List<string>();

                    if (doneFilteredUsers.Count > 0)
                    {
                        int maxUsernameLength = doneFilteredUsers.Max(u => u.Username.Length);
                        int maxValueLength = doneFilteredUsers.Max(u => $"{u.Value}".Length);

                        strings = doneFilteredUsers
                           .Select(u =>
                           $"`{u.Username}`" +
                           $"{u.Username.PadRight(maxUsernameLength, invisibleChar).Replace(u.Username, string.Empty)} " +
                           $"{u.RankEmoji} {$"`{u.Value}`".PadLeft(maxValueLength + 2, invisibleChar)} ").ToList();
                    }

                    List<EmbedBuilder> embeds = new List<EmbedBuilder>() { embed }
                        .Concat(
                            new EmbedBuilder() { Color = Discord.Color.Green }
                            .AddStrings("Leaderboard", strings)
                        )
                        .ToList();

                    var th = ch.Threads.Any(x => x.Name == achievementData.name)
                        ? ch.Threads.First(x => x.Name == achievementData.name)
                        : await ch.CreateThreadAsync(achievementData.name, ThreadType.PublicThread, ThreadArchiveDuration.OneWeek);

                    embeds.ForEach(x => TryingSendEmbedToThread(x, th, embeds.IndexOf(x)));
                    var Th_messages = (await th.GetMessagesAsync().FlattenAsync())
                        .Where(x => x.Author.IsBot)
                        .Reverse()
                        .ToList();
                    if (Th_messages.Count > embeds.Count)
                    {
                        for (int a = embeds.Count; a < Th_messages.Count; a++)
                        {
                            await (Th_messages[a] as IUserMessage).DeleteAsync();
                        }
                    }
                }
            }

            var messages = await ch.GetMessagesAsync().FlattenAsync();
            foreach (var message in messages.Where(x => x.Source.ToString() == "System").ToList())
            {
                await message.DeleteAsync();
            }

        }


        private static async void TryingSendEmbedToThread(EmbedBuilder embed, SocketThreadChannel th, int count)
        {
            var messages = (await th.GetMessagesAsync().FlattenAsync())
                .Where(x => x.Author.IsBot)
                .Reverse()
                .ToList();

            if (messages.Count > count) await (messages[count] as IUserMessage).ModifyAsync(x => x.Embed = embed.Build());
            else await th.SendMessageAsync(embed: embed.Build());
        }
        private static async void TryingSendEmbedToChannel(EmbedBuilder embed, SocketTextChannel ch, int count)
        {
            var messages = (await ch.GetMessagesAsync().FlattenAsync())
                .Where(x => x.Author.IsBot)
                .Reverse()
                .ToList();

            if (messages.Count > count) await (messages[count] as IUserMessage).ModifyAsync(x => x.Embed = embed.Build());
            else await ch.SendMessageAsync(embed: embed.Build());
        }
    }
}

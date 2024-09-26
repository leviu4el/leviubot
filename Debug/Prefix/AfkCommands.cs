using Discord.Commands;
using Discord.WebSocket;
using System.Text.Json;

namespace Debug.Prefix
{
    public class AfkCommands : ModuleBase<SocketCommandContext> //Old code from back project
    {
        private static string FilePath = Path.Combine(Source.Program.baseDir, "usersettings.json");
        private static List<UserSettings> userSettings = new List<UserSettings>();

        [Command("afk")]
        public async Task AfkCommand(params string[] message) { AfkTypeCommand("afk", Context); }

        [Command("gn")]
        public async Task GnCommand(params string[] message) { AfkTypeCommand("gn", Context); }

        [Command("brb")]
        public async Task BrbCommand(params string[] message) { AfkTypeCommand("brb", Context); }

        [Command("shower")]
        public async Task ShowerCommand(params string[] message) { AfkTypeCommand("shower", Context); }

        [Command("poop")]
        public async Task PoopCommand(params string[] message) { AfkTypeCommand("poop", Context); }

        [Command("lurk")]
        public async Task LurkCommand(params string[] message) { AfkTypeCommand("lurk", Context); }

        [Command("work")]
        public async Task WorkCommand(params string[] message) { AfkTypeCommand("work", Context); }

        [Command("study")]
        public async Task StudyCommand(params string[] message) { AfkTypeCommand("study", Context); }

        [Command("nap")]
        public async Task NapCommand(params string[] message) { AfkTypeCommand("nap", Context); }

        [Command("food")]
        public async Task FoodCommand(params string[] message) { AfkTypeCommand("food", Context); }

        [Command("tetris")]
        public async Task TetrisCommand(params string[] message) { AfkTypeCommand("tetris", Context); }

        [Command("cry")]
        public async Task CryCommand(params string[] message) { AfkTypeCommand("cry", Context); }
        private async Task AfkTypeCommand(string AfkType, SocketCommandContext Context)
        {
            DateTimeOffset messageSentTime = Context.Message.CreatedAt;
            UpdateUserSettings(Context.User.Id, AfkType, messageSentTime.Ticks / TimeSpan.TicksPerSecond);

            switch (AfkType)
            {
                case "afk":
                    await ReplyAsync($"{Context.User.Username} is now AFK");
                    break;
                case "gn":
                    await ReplyAsync($"{Context.User.Username} is now sleeping");
                    break;
                case "brb":
                    await ReplyAsync($"{Context.User.Username} is going to be right back");
                    break;
                case "shower":
                    await ReplyAsync($"{Context.User.Username} is now taking a shower");
                    break;
                case "poop":
                    await ReplyAsync($"{Context.User.Username} is now pooping");
                    break;
                case "lurk":
                    await ReplyAsync($"{Context.User.Username} is now lurking");
                    break;
                case "work":
                    await ReplyAsync($"{Context.User.Username} is now working");
                    break;
                case "study":
                    await ReplyAsync($"{Context.User.Username} is now studying");
                    break;
                case "nap":
                    await ReplyAsync($"{Context.User.Username} is now taking a nap");
                    break;
                case "food":
                    await ReplyAsync($"{Context.User.Username} is now eating");
                    break;
                case "tetris":
                    await ReplyAsync($"{Context.User.Username} is currently playing Tetris");
                    break;
                case "cry":
                    await ReplyAsync($"{Context.User.Username} is currently crying");
                    break;
                default:
                    await ReplyAsync($"{AfkType} err?");
                    break;
            }
        }

        [Command("rafk")]
        public async Task RAfkCommand(params string[] message)
        {
            await RAfkTypeCommand(Context);
        }

        private async Task RAfkTypeCommand(SocketCommandContext Context)
        {
            var user = userSettings.FirstOrDefault(n => n.UserId == Context.User.Id);
            if (user == null)
            {
                await ReplyAsync($"ERROR: No AFK status found for {Context.User.Username}");
                return;
            }

            long dif = Context.Message.CreatedAt.Ticks / TimeSpan.TicksPerSecond - user.UsedTime;
            TimeSpan timeSpan = TimeSpan.FromSeconds(dif);

            if ((int)timeSpan.TotalMinutes < 5)
            {
                await ReplyAsync($"{Context.User.Username}, Your status has been resumed.");

                user.UsedTime = Context.Message.CreatedAt.Ticks / TimeSpan.TicksPerSecond;
                user.AfkEnabled = true;

                SaveUserSettings();
            }
            else
            {
                await ReplyAsync($"{Context.User.Mention}, NOPE, you can't resume your status.");
            }
        }

        private void UpdateUserSettings(ulong userId, string afkType, long afkTime)
        {
            UserSettings? user = userSettings.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
            {
                user = new UserSettings
                {
                    UserId = userId,
                    AfkType = afkType,
                    AfkTime = afkTime,
                    UsedTime = 0,
                    AfkEnabled = true
                };
                userSettings.Add(user);
            }
            else
            {
                user.AfkType = afkType;
                user.AfkTime = afkTime;
                user.UsedTime = 0;
                user.AfkEnabled = true;
            }

            SaveUserSettings();
        }

        public static List<UserSettings> LoadUserSettings()
        {
            if (File.Exists(FilePath))
            {
                try
                {
                    var json = File.ReadAllText(FilePath);
                    return JsonSerializer.Deserialize<List<UserSettings>>(json) ?? new List<UserSettings>();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to load user settings: {ex.Message}");
                }
            }
            return new List<UserSettings>();
        }
        public static void SaveUserSettings()
        {
            try
            {
                var json = JsonSerializer.Serialize(userSettings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(FilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save user settings: {ex.Message}");
            }
        }
        public static async Task AfkResponses(SocketMessage arg)
        {
            SocketGuild guild = (arg.Channel as SocketGuildChannel).Guild;
            var message = arg as SocketUserMessage;
            int argPos = 0;

            if (message == null) return;
            if (arg.Author.IsBot || arg.Author.IsWebhook) return;
            if (message.HasCharPrefix(Source.Program.config["prefix"][0], ref argPos)) return;

            UserSettings user = userSettings.FirstOrDefault(n => n.UserId == arg.Author.Id);

            if (user == null || !user.AfkEnabled) return;

            long dif = arg.CreatedAt.Ticks / TimeSpan.TicksPerSecond - user.AfkTime;
            TimeSpan timeSpan = TimeSpan.FromSeconds(dif);

            string reply = string.Empty;

            switch (user.AfkType)
            {
                case "afk":
                    reply = $"{arg.Author.Username} is no longer AFK: ";
                    break;

                case "gn":
                    if (timeSpan.TotalMinutes <= 5) reply = $"{arg.Author.Username} changed their mind: ";
                    else if (timeSpan.TotalMinutes > 5 && timeSpan.TotalHours < 6) reply = $"{arg.Author.Username} got average amount of sleep high schoolers get: ";
                    else if (timeSpan.TotalHours >= 6 && timeSpan.TotalHours < 10) reply = $"{arg.Author.Username} feels well rested: ";
                    else if (timeSpan.TotalHours >= 10 && timeSpan.TotalHours < 12) reply = $"{arg.Author.Username} definitely overslept: ";
                    else if (timeSpan.TotalHours >= 12 && timeSpan.TotalHours < 15) reply = $"{arg.Author.Username} spent the entire day in bed: ";
                    else if (timeSpan.TotalHours >= 15 && timeSpan.TotalHours < 24) reply = $"{arg.Author.Username} probaly forgot to check the chat all day: ";
                    else if (timeSpan.TotalHours >= 24 && timeSpan.TotalDays < 2) reply = $"{arg.Author.Username} should've used !afk instead: ";
                    else reply = $"{arg.Author.Username} came out of a coma: ";
                    break;

                case "brb":
                    reply = $"{arg.Author.Username} just got back: ";
                    break;

                case "shower":
                    if (timeSpan.TotalMinutes <= 5) reply = $"{arg.Author.Username} forgot their towel: ";
                    else reply = $"{arg.Author.Username} is now squeacky clean: ";
                    break;

                case "poop":
                    reply = $"{arg.Author.Username} forgot to flush: ";
                    break;

                case "lurk":
                    reply = $"{arg.Author.Username} has stopped lurking: ";
                    break;

                case "work":
                    if (timeSpan.TotalMinutes <= 5) reply = $"{arg.Author.Username} should be working harder: ";
                    else reply = $"{arg.Author.Username} finished their work: ";
                    break;

                case "study":
                    if (timeSpan.TotalMinutes <= 25) reply = $"{arg.Author.Username} hasn't learnt anything: ";
                    else reply = $"{arg.Author.Username} is now smarter than most of this chat: ";
                    break;

                case "nap":
                    reply = $"{arg.Author.Username} feels much better ";
                    break;

                case "food":
                    if (timeSpan.TotalMinutes <= 25) reply = $"{arg.Author.Username} finished eating: ";
                    else reply = $"{arg.Author.Username} had to cook their food: ";
                    break;

                case "tetris":
                    reply = $"{arg.Author.Username} has finished playing Tetris: ";
                    break;

                case "cry":
                    reply = $"{arg.Author.Username} has finished crying for: ";
                    break;
            }

            string timeString = timeSpan switch
            {
                { TotalDays: > 1 } => $"{timeSpan.Days}d {timeSpan.Hours}h {timeSpan.Minutes}m",
                { TotalHours: > 1 } => $"{timeSpan.Hours}h {timeSpan.Minutes}m",
                { TotalMinutes: > 1 } => $"{timeSpan.Minutes}m {timeSpan.Seconds}s",
                { TotalSeconds: > 1 } => $"{timeSpan.Seconds}s",
                _ => $"{timeSpan.TotalMilliseconds}ms"
            };

            await arg.Channel.SendMessageAsync(reply + timeString);

            user.UsedTime = arg.CreatedAt.Ticks / TimeSpan.TicksPerSecond;
            user.AfkEnabled = false;

            SaveUserSettings();
        }
        public class UserSettings
        {
            public ulong UserId { get; set; }
            public string AfkType { get; set; }
            public long AfkTime { get; set; }
            public long UsedTime { get; set; }
            public bool AfkEnabled { get; set; }
        }
    }
}

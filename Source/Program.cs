﻿using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Source.DataClasses;
using System.Text.Json.Nodes;

namespace Source
{
    public class Program
    {
        public static IConfiguration config;
        public static string baseDir;
        public static List<StoredUser> storedUsers = new List<StoredUser>();


        public static Dictionary<LeagueRank, string> RankEmoji = new Dictionary<LeagueRank, string>();
        public static Dictionary<ArType, string> ArEmoji = new Dictionary<ArType, string>();

        private static void Main(string[] args) { }
        public static void Setup()
        {
            baseDir = GetBaseDir();
            config = GetConfig();
            storedUsers = LoadStoredUsers();
        }

        public async static Task LoadEmoji(IReadOnlyCollection<Discord.Emote> emotes)
        {
            ArEmoji = emotes
               .Where(emote => emote.Name.StartsWith(nameof(ArType)))
               .Where(emote => Enum.GetNames(typeof(ArType)).Select(x => nameof(ArType) + x).Any(x => x == emote.Name))
               .Select(emote => new { Emote = emote, ArType = (ArType)Enum.Parse(typeof(ArType), emote.Name.Substring(nameof(ArType).Length)) })
               .OrderBy(x => x.ArType)
               .ToDictionary(x => x.ArType, x => $"<:{x.Emote.Name}:{x.Emote.Id}>");

            RankEmoji = emotes
               .Where(emote => emote.Name.StartsWith(nameof(LeagueRank)))
               .Where(emote => Enum.GetNames(typeof(LeagueRank)).Select(x => nameof(LeagueRank) + x).Any(x => x == emote.Name))
               .Select(emote => new { Emote = emote, ArType = (LeagueRank)Enum.Parse(typeof(LeagueRank), emote.Name.Substring(nameof(LeagueRank).Length)) })
               .OrderBy(x => x.ArType)
               .ToDictionary(x => x.ArType, x => $"<:{x.Emote.Name}:{x.Emote.Id}>");
        }

        private static string GetBaseDir()
        {
            string baseDir = AppContext.BaseDirectory;
            DirectoryInfo directory = new DirectoryInfo(baseDir);

            while (directory != null)
            {
                string configFilePath = Path.Combine(directory.FullName, "config.yml");

                if (File.Exists(configFilePath)) return directory.FullName;
                directory = directory.Parent;
            }
            throw new Exception("config.yml does not exist");
        }
        private static IConfiguration GetConfig()
        {
            return new ConfigurationBuilder()
                .SetBasePath(baseDir)
                .AddYamlFile("config.yml")
                .Build();
        }
        private static List<StoredUser> LoadStoredUsers()
        {
            string path = Path.Combine(baseDir, "users.json");
            if (!File.Exists(path)) File.Create(path);
            return JsonConvert.DeserializeObject<List<StoredUser>>(File.ReadAllText(path)) ?? new List<StoredUser>();
        }
        public static void SaveUser(ulong discord_id, string tetris_id)
        {
            storedUsers.Add(new StoredUser
            {
                discord_id = discord_id,
                tetris_id = tetris_id
            });
            string json = JsonConvert.SerializeObject(Program.storedUsers, Formatting.Indented);
            File.WriteAllText(Path.Combine(baseDir,"users.json"), json);
        }
        public class StoredUser
        {
            public ulong discord_id;
            public string tetris_id;
        }

        public static dynamic GetResult(string link, bool spamthing = false)
        {
            using (var client = new HttpClient())
            {
                if (spamthing) client.DefaultRequestHeaders.Add("X-Session-ID", "She's not that flat");
                client.DefaultRequestHeaders.Add("User-Agent", "leviudude thing");


                var endpoint = new Uri(link);
                var result = client.GetAsync(endpoint).Result;
                var json = result.Content.ReadAsStringAsync().Result;

                return JsonConvert.DeserializeObject<dynamic>(json)!;
            }
        }
    }
}
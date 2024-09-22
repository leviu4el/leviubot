using leviubot.DataClasses;
using Newtonsoft.Json;
using static leviubot.DataClasses.TetrIo;
using Newtonsoft.Json.Linq;
using Discord.WebSocket;
using static Achievement;

namespace leviubot.DataClasses
{
    public class TetrIo
    {
        #region const

        public static List<Rank> Ranks = Enum.GetValues(typeof(Rank)).Cast<Rank>().ToList();
        public static Dictionary<Rank,string> RankEmoji = new Dictionary<Rank, string>();
        public static Dictionary<ArType, string> ArEmoji = new Dictionary<ArType, string>();
        //public static List<ulong> RankIds = new List<ulong>();

        public async static Task LoadRanksData(SocketGuild rolesGuild, IReadOnlyCollection<Discord.Emote> emotes)
        {
            if (File.Exists("messages.json")) Program._storedMessages = JsonConvert.DeserializeObject<List<StoredData>>(File.ReadAllText("messages.json"));
            if (File.Exists("users.json")) Program._storedUsers = JsonConvert.DeserializeObject<List<StoredUser>>(File.ReadAllText("users.json"));

            RankEmoji = emotes
                .Where(emote => emote.Name.StartsWith(nameof(Rank)))
                .Where(emote => Enum.GetNames(typeof(Rank)).Select(x => nameof(Rank) + x).Any(x => x == emote.Name))
                .Select(emote => new { Emote = emote, Rank = (Rank)Enum.Parse(typeof(Rank), emote.Name.Substring(nameof(Rank).Length)) })
                .OrderBy(x => x.Rank)
                .ToDictionary(x => x.Rank, x => $"<:{x.Emote.Name}:{x.Emote.Id}>");

            ArEmoji = emotes
                .Where(emote => emote.Name.StartsWith(nameof(ArType)))
                .Where(emote => Enum.GetNames(typeof(ArType)).Select(x => nameof(ArType) + x).Any(x => x == emote.Name))
                .Select(emote => new { Emote = emote, ArType = (ArType)Enum.Parse(typeof(ArType), emote.Name.Substring(nameof(ArType).Length)) })
                .OrderBy(x => x.ArType)
                .ToDictionary(x => x.ArType, x => $"<:{x.Emote.Name}:{x.Emote.Id}>");
        }

        private static readonly double apmweight = 1; // All of the below are weights to do with the versus graph area and the area stat.
        private static readonly double ppsweight = 45;
        private static readonly double vsweight = 0.444;
        private static readonly double appweight = 185;
        private static readonly double dssweight = 175;
        private static readonly double dspweight = 450;
        private static readonly double dsappweight = 140;
        private static readonly double vsapmweight = 60;
        private static readonly double ciweight = 1.25;
        private static readonly double geweight = 315;

        private static readonly double apmsrw = 0; // All of the below are weights for the stat rank (or sr) stat and the esttr / estglicko doubleiables.
        private static readonly double ppssrw = 135;
        private static readonly double vssrw = 0;
        private static readonly double appsrw = 290;
        private static readonly double dsssrw = 0;
        private static readonly double dspsrw = 700;
        private static readonly double garbageeffisrw = 0;

        internal static dynamic GetResult(string link, bool spamthing = false)
        {
            using (var client = new HttpClient())
            {
                //client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.0.0 Safari/537.36");
                
                if (spamthing) client.DefaultRequestHeaders.Add("X-Session-ID", "She's not that flat");

                client.DefaultRequestHeaders.Add("User-Agent", "leviudude thing");


                var endpoint = new Uri(link);
                var result = client.GetAsync(endpoint).Result;
                var json = result.Content.ReadAsStringAsync().Result;

                return JsonConvert.DeserializeObject<dynamic>(json)!;
            }
        }

        #endregion

        #region NerdStats
        public static NerdStats NerdTime(double apm, double pps, double vs)
        {
            double app = apm / 60 / pps;
            double dss = vs / 100 - apm / 60;
            double dsp = dss / pps;
            double dsapp = dsp + app;
            double vsapm = vs / apm;
            double ci = dsp * 150 + (vsapm - 2) * 50 + (0.6 - app) * 125;
            double ge = app * dss / pps * 2;
            double wapp = app - 5 * Math.Tan((ci / -30 + 1) * Math.PI / 180);
            double area = apm * apmweight + pps * ppsweight + vs * vsweight + app * appweight + dss * dssweight + dsp * dspweight + ge * geweight;

            double srarea = apm * apmsrw + pps * ppssrw + vs * vssrw + app * appsrw + dss * dsssrw + dsp * dspsrw + ge * garbageeffisrw;
            double sr = 11.2 * Math.Atan((srarea - 93) / 130) + 1;
            if (sr <= 0) sr = 0.001;
            double estglicko = 0.000013 * Math.Pow(pps * (150 + (vsapm - 1.66) * 35) + app * 290 + dsp * 700, 3) - 0.0196 * Math.Pow(pps * (150 + (vsapm - 1.66) * 35) + app * 290 + dsp * 700, 2) + 12.645 * (pps * (150 + (vsapm - 1.66) * 35) + app * 290 + dsp * 700) - 1005.4;
            double esttr = 25000 / (1 + 10 * ((1500 - estglicko) * Math.PI / Math.Sqrt(3 * Math.Log(10) * 2 * 60 * 2 + 2500 * (64 * Math.PI * 2 + 147 * Math.Log(10) * 2))));
            //double atr = esttr - tr;

            double opener = Math.Round((apm / srarea / (0.069 * Math.Pow(1.0017, Math.Pow(sr, 5) / 4700) + sr / 360) - 1 + (pps / srarea / (0.0084264 * Math.Pow(2.14, -2 * (sr / 2.7 + 1.03)) - sr / 5750 + 0.0067) - 1) * 0.75 + (vsapm / (-Math.Pow((sr - 16) / 36, 2) + 2.133) - 1) * -10 + (app / (0.1368803292 * Math.Pow(1.0024, Math.Pow(sr, 5) / 2800) + sr / 54) - 1) * 0.75 + (dsp / (0.02136327583 * Math.Pow(14, (sr - 14.75) / 3.9) + sr / 152 + 0.022) - 1) * -0.25) / 3.5 + 0.5, 4);
            double plonk = Math.Round((ge / (sr / 350 + 0.005948424455 * Math.Pow(3.8, (sr - 6.1) / 4) + 0.006) - 1 + (app / (0.1368803292 * Math.Pow(1.0024, Math.Pow(sr, 5) / 2800) + sr / 54) - 1) + (dsp / (0.02136327583 * Math.Pow(14, (sr - 14.75) / 3.9) + sr / 152 + 0.022) - 1) * 0.75 + (pps / srarea / (0.0084264 * Math.Pow(2.14, -2 * (sr / 2.7 + 1.03)) - sr / 5750 + 0.0067) - 1) * -1) / 2.73 + 0.5, 4);
            double stride = Math.Round(((apm / srarea / (0.069 * Math.Pow(1.0017, Math.Pow(sr, 5) / 4700) + sr / 360) - 1) * -0.25 + (pps / srarea / (0.0084264 * Math.Pow(2.14, -2 * (sr / 2.7 + 1.03)) - sr / 5750 + 0.0067) - 1) + (app / (0.1368803292 * Math.Pow(1.0024, Math.Pow(sr, 5) / 2800) + sr / 54) - 1) * -2 + (dsp / (0.02136327583 * Math.Pow(14, (sr - 14.75) / 3.9) + sr / 152 + 0.022) - 1) * -0.5) * 0.79 + 0.5, 4);
            double infds = Math.Round((dsp / (0.02136327583 * Math.Pow(14, (sr - 14.75) / 3.9) + sr / 152 + 0.022) - 1 + (app / (0.1368803292 * Math.Pow(1.0024, Math.Pow(sr, 5) / 2800) + sr / 54) - 1) * -0.75 + (apm / srarea / (0.069 * Math.Pow(1.0017, Math.Pow(sr, 5) / 4700) + sr / 360) - 1) * 0.5 + (vsapm / (-Math.Pow((sr - 16) / 36, 2) + 2.133) - 1) * 1.5 + (pps / srarea / (0.0084264 * Math.Pow(2.14, -2 * (sr / 2.7 + 1.03)) - sr / 5750 + 0.0067) - 1) * 0.5) * 0.9 + 0.5, 4);

            return new NerdStats()
            {
                app = app,
                dss = dss,
                dsp = dsp,
                wapp = wapp,

                dsapp = dsapp,
                vsapm = vsapm,

                ci = ci,
                ge = ge,

                opener = opener,
                plonk = plonk,
                stride = stride,
                infds = infds,
            };
        }
        public class NerdStats
        {
            public double app;
            public double dss;
            public double dsp;
            public double wapp;

            public double dsapp;
            public double vsapm;

            public double ci;
            public double ge;

            public double opener;
            public double plonk;
            public double stride;
            public double infds;
        }
        #endregion
    }
}

public class TetrisUser
{
    public string uuid { get; private set; }

    public UserInfo info { get; private set; }
    public FourtyLines fourtyLines { get; private set; }
    public Blitz blitz { get; private set; }
    public QuickPlay quickPlay { get; private set; }
    public ExpertQuickPlay expertQuickPlay { get; private set; }
    public TetraLeague tetraLeague { get; private set; }
    public Zen zen { get; private set; }
    public List<Achievement> achievements { get; private set; }

    public bool spamThing { get; private set; }

    public TetrisUser(string? username, ulong id, bool spamthing = false)
    {
        username ??= CheckDiscord(id);
        if (username == null)
            throw new LeviuException(title: "User not found");

        uuid = username.ToLower();

        info = new UserInfo();
        fourtyLines = new FourtyLines();
        blitz = new Blitz(); 
        quickPlay = new QuickPlay(); 
        expertQuickPlay = new ExpertQuickPlay(); 
        tetraLeague = new TetraLeague(); 
        zen = new Zen();
        achievements = new List<Achievement>();


        spamThing = spamthing;
    }
    public static string CheckDiscord(ulong discordId)
    {
        if (Program._storedUsers.Any(x => x.user_id == discordId))
        {
            return Program._storedUsers.First(x => x.user_id == discordId).tetris_id;
        }

        var jsonObject = TetrIo.GetResult($"https://ch.tetr.io/api/users/search/discord:{discordId}");

        if (!Convert.ToBoolean(jsonObject.success) || jsonObject.data == null)
            throw new LeviuException(
                parameters: new List<ExceptionParameter>()
                {
                    new ExceptionParameter{ name = nameof(discordId), value = discordId.ToString()},
                    new ExceptionParameter{ name = nameof(jsonObject.success), value = jsonObject.success},
                    new ExceptionParameter{ name = nameof(jsonObject.data), value = jsonObject.data},
                },
                title: "Search by discord connection fails"
            );

        #region add extra user
        Program._storedUsers.Add(new StoredUser
        {
            user_id = discordId,
            tetris_id = jsonObject.data.user._id
        });
        string json = JsonConvert.SerializeObject(Program._storedUsers, Formatting.Indented);
        File.WriteAllText("users.json", json);
        #endregion

        return jsonObject.data.user._id;
    }
    public void GetUserInfo()
    {
        var data = TetrIo.GetResult($"https://ch.tetr.io/api/users/{uuid}", spamThing).data;
        if (data == null) return;

        info.username = data.username;
        info.id = data._id;
        info.avatar = data.avatar_revision;
        info.banner = data.banner_revision;

        info.gametime = ToDouble(data.gametime);
        info.xp = ToDouble(data.xp);

        info.discordId = data.connections.discord?.id;


        info.ar = data.ar;


        Dictionary<ArType, string> arValues = new Dictionary<ArType, string>()
        {
            { ArType.Top3, "t3" },
            { ArType.Top5, "t5" },
            { ArType.Top10, "t10" },
            { ArType.Top25, "t25" },
            { ArType.Top50, "t50" },
            { ArType.Top100, "t100" },
            { ArType.Issued, "100" },
            { ArType.Diamond, "5" },
            { ArType.Platinum, "4" },
            { ArType.Gold, "3" },
            { ArType.Silver, "2" },
            { ArType.Bronze, "1" },
        };
        Dictionary<ArType, int> arCounts = new Dictionary<ArType, int>();
        foreach (var type in Enum.GetValues(typeof(ArType)).Cast<ArType>().ToList())
        {
            if (arValues.TryGetValue(type, out string arString))
            {
                var value = data.ar_counts[arString];
                if (value != null)
                {
                    arCounts.Add(type, ToInt(value));
                }
            }
        }
        info.arCounts = arCounts;
    }

    public void GetAll() {
        var jsonObject = TetrIo.GetResult($"https://ch.tetr.io/api/users/{uuid}/summaries", spamThing);
        FetchFourtyLines      (jsonObject.data["40l"]);
        FetchBlitz            (jsonObject.data["blitz"]);
        FetchQuickPlay        (jsonObject.data["zenith"]);
        FetchExpertQuickPlay  (jsonObject.data["zenithex"]);
        FetchTetraLeague      (jsonObject.data["league"]);
        FetchZen              (jsonObject.data["zen"]);
        FetchAchievements     (jsonObject.data["achievements"]);
    }

    public void GetFourtyLines() =>     FetchFourtyLines        (TetrIo.GetResult($"https://ch.tetr.io/api/users/{uuid}/summaries/40l", spamThing).data);
    public void GetBlitz() =>           FetchBlitz              (TetrIo.GetResult($"https://ch.tetr.io/api/users/{uuid}/summaries/blitz", spamThing).data);
    public void GetQuickPlay() =>       FetchQuickPlay          (TetrIo.GetResult($"https://ch.tetr.io/api/users/{uuid}/summaries/zenith", spamThing).data);
    public void GetExpertQuickPlay() => FetchExpertQuickPlay    (TetrIo.GetResult($"https://ch.tetr.io/api/users/{uuid}/summaries/zenithex", spamThing).data);
    public void GetTetraLeague() =>     FetchTetraLeague        (TetrIo.GetResult($"https://ch.tetr.io/api/users/{uuid}/summaries/league", spamThing).data);
    public void GetZen() =>             FetchZen                (TetrIo.GetResult($"https://ch.tetr.io/api/users/{uuid}/summaries/zen", spamThing).data);
    public void GetAchievements() =>    FetchAchievements       (TetrIo.GetResult($"https://ch.tetr.io/api/users/{uuid}/summaries/achievements", spamThing).data);


    private void FetchFourtyLines(dynamic data)
    {
        if (data == null) return;
        fourtyLines.rank = ToInt(data.rank);

        var record = data.record;
        info.username ??= record.user.username;
        info.id ??= record.user.id;
        info.avatar ??= record.user.avatar_revision;
        info.banner ??= record.user.banner_revision;

        fourtyLines.pps = ToInt(record.results.aggregatestats.pps);
        fourtyLines.inputs = ToInt(record.results.stats.inputs);
        fourtyLines.holds = ToInt(record.results.stats.holds);
        fourtyLines.piecesplaced = ToInt(record.results.stats.piecesplaced);

        fourtyLines.finaltime = ToDouble(record.results.stats.finaltime);
    }
    private bool FetchBlitz(dynamic data)
    {
        if (data == null) return false;
        blitz.rank = ToInt(data.rank);

        var record = data.record;
        info.username ??= record.user.username;
        info.id ??= record.user.id;
        info.avatar ??= record.user.avatar_revision;
        info.banner ??= record.user.banner_revision;

        blitz.pps = ToInt(record.results.aggregatestats.pps);
        blitz.inputs = ToInt(record.results.stats.inputs);
        blitz.holds = ToInt(record.results.stats.holds);
        blitz.piecesplaced = ToInt(record.results.stats.piecesplaced);
        blitz.allclear = ToInt(record.results.stats.clears.allclear);

        blitz.score = ToInt(record.results.stats.score);

        return true;
    }
    private void FetchQuickPlay(dynamic data)
    {
        if (data == null) return;
    }
    private void FetchExpertQuickPlay(dynamic data)
    {
        if (data == null) return;
    }
    private void FetchTetraLeague(dynamic data)
    {
        if (data == null) return;
        tetraLeague.seasons = [SeasonData.From(data)]; // current season

        var past = data.past as JObject;
        foreach (var item in past)
        {
            tetraLeague.seasons.Add(SeasonData.From(item.Value as dynamic));
        }

        tetraLeague.seasons.First().season = tetraLeague.seasons.Max(x => x.season) + 1;  // current season
        tetraLeague.currentSeason = tetraLeague.seasons.First();

    }
    private void FetchZen(dynamic data)
    {
        if (data == null) return;
        zen.level = ToInt(data.level);
        zen.score = ToInt(data.score);
    }
    private void FetchAchievements(dynamic data)
    {
        if (data == null) return;
        var allAchievements = data as JToken;
        foreach (dynamic achievement in allAchievements)
        {
            achievements.Add(Achievement.From(achievement));
        }

    }
}
public class UserInfo
{
    public string username { get; set; }
    public string id { get; set; }

    public ulong? discordId { get; set; }

    public string? avatar { get; set; }
    public string? banner { get; set; }

    public int ar { get; set; }
    public Dictionary<ArType, int> arCounts { get; set; }

    public double gametime { get; set; }
    public double xp { get; set; }
} // done
public class FourtyLines
{
    public int rank { get; set; }

    public double pps { get; set; }

    public int inputs { get; set; }
    public int holds { get; set; }
    public int piecesplaced { get; set; }

    public double finaltime { get; set; }
} // done
public class Blitz
{
    public int rank { get; set; }

    public double pps { get; set; }
    public int inputs { get; set; }
    public int holds { get; set; }
    public int piecesplaced { get; set; }

    public int allclear { get; set; }
    public int score { get; set; }
} // done
public class Zen
{
    public int level { get; set; }
    public int score { get; set; }
} // done
public class TetraLeague
{
    public SeasonData currentSeason { get; set; }
    public List<SeasonData> seasons { get; set; }
} // done
public class SeasonData
{
    public int season { get; set; }
    public int gamesplayed { get; set; }
    public int gameswon { get; set; }

    public double glicko { get; set; }
    public double rd { get; set; }

    public double tr { get; set; }
    public int rank { get; set; }
    public int bestrank { get; set; }

    public double apm { get; set; }
    public double pps { get; set; }
    public double vs { get; set; }
    public NerdStats? nerdStats { get; set; }

    public int standing { get; set; }

    public static SeasonData From(dynamic data)
    {
        Log.println(data.ToString());
        return new SeasonData
        {
            season = ToInt(data.season),
            gamesplayed = ToInt(data.gamesplayed),
            gameswon = ToInt(data.gameswon),
            glicko = ToDouble(data.glicko),
            rd = ToDouble(data.rd),
            tr = ToDouble(data.tr),
            //rank = data.rank,
            //bestrank = data.bestrank,
            apm = ToDouble(data.apm),
            pps = ToDouble(data.pps),
            vs = ToDouble(data.vs),
            standing = data.standing != null ? ToInt(data.standing) : ToInt(data.placement),
            nerdStats = NerdTime(ToDouble(data.apm), ToDouble(data.pps), ToDouble(data.vs))
        };
    }
} // done
public class ExpertQuickPlay
{
}
public class QuickPlay
{
}
public class Achievement
{
    public int id;
    public string name;
    public string description;

    public dynamic value;
    public dynamic mainValue;
    public ArType rank;

    public int pos;

    public rt rankType;
    public vt valueType;
    public art arType;

    public bool unlocked;

    public enum art
    {
        UNRANKED,
        RANKED,
        COMPETITIVE,
    }

    public enum rt
    {
        Error,

        PERCENTILE,
        ISSUE,
        ZENITH,
        PERCENTILELAX,
        PERCENTILEVLAX,
        PERCENTILEMLAX
    }

    public enum vt
    {
        NONE,
        NUMBER,
        TIME,
        TIME_INV,
        FLOOR,
        ISSUE,
        NUMBER_INV
    }


    internal static Achievement From(dynamic achievement)
    {
        Dictionary<int, ArType> arValues = new Dictionary<int, ArType>()
        {
            { 100, ArType.Issued },
            { 5,   ArType.Diamond },
            { 4,   ArType.Platinum },
            { 3,   ArType.Gold },
            { 2,   ArType.Silver },
            { 1,   ArType.Bronze },
            { 0,   ArType.None },
        };

        art arType = (art)ToInt(achievement.art);
        rt rankType = (rt)ToInt(achievement.rt);
        vt valueType = (vt)ToInt(achievement.vt);

        ArType rank = ArType.None;

        int pos = ToInt(achievement.pos);


        if (achievement.rank != null) rank = arValues[ToInt(achievement.rank)];
        if (arType == art.COMPETITIVE && pos >= 0)
        {
            if (pos < 100) rank = ArType.Top100;
            if (pos < 50) rank = ArType.Top50;
            if (pos < 25) rank = ArType.Top25;
            if (pos < 10) rank = ArType.Top10;
            if (pos < 5) rank = ArType.Top5;
            if (pos < 3) rank = ArType.Top3;
        }

        return new Achievement
        {
            id = achievement.k,
            name = achievement.name,
            description = achievement.desc,

            value = valueType == vt.ISSUE
                ? achievement.pos
                : achievement.rank != null
                    ? AchievementData.Convert(achievement.v, valueType) 
                    : "locked",
            mainValue = achievement.v ??= achievement.pos,

            arType = arType,
            valueType = valueType,
            rankType = rankType,

            pos = pos,

            unlocked = achievement.rank != null,
            rank = rank,
        };
    }
} // done
public class AchievementData
{
    public int id;
    public string name;
    public string description;

    public Achievement.rt rankType;
    public Achievement.vt valueType;
    public Achievement.art arType;

    public Dictionary<ArType, dynamic> cutoffs { get; set; }

    public static AchievementData From(dynamic data)
    {
        var achievement = data.achievement;
        var leaderboard = data.leaderboard as JToken;
        var cutoffs = data.cutoffs;

        art arType = (art)ToInt(achievement.art);
        rt rankType = (rt)ToInt(achievement.rt);
        vt valueType = (vt)ToInt(achievement.vt);


        Dictionary<ArType, dynamic> AchievementData = new Dictionary<ArType, dynamic>();

        if (rankType != rt.ISSUE)
        {
            if (rankType != rt.ZENITH)
            {
                AchievementData = new Dictionary<ArType, dynamic>()
                {
                    { ArType.Diamond, cutoffs.diamond },
                    { ArType.Platinum, cutoffs.platinum },
                };
                if (rankType == rt.PERCENTILE)
                    AchievementData = AchievementData.Concat(new Dictionary<ArType, dynamic>() {
                        { ArType.Gold, cutoffs.gold },
                        { ArType.Silver, cutoffs.silver },
                        { ArType.Bronze, cutoffs.bronze }
                    }).ToDictionary();

                else if (rankType == rt.PERCENTILELAX || rankType == rt.PERCENTILEMLAX)
                    AchievementData = AchievementData.Concat(new Dictionary<ArType, dynamic>() {
                        { ArType.Gold, cutoffs.gold },
                    }).ToDictionary();

            }
            if (arType == art.COMPETITIVE)
            {
                AchievementData[ArType.Top100] = (leaderboard[99] as dynamic).v;
                AchievementData[ArType.Top50] = (leaderboard[49] as dynamic).v;
                AchievementData[ArType.Top25] = (leaderboard[24] as dynamic).v;
                AchievementData[ArType.Top10] = (leaderboard[9] as dynamic).v;
                AchievementData[ArType.Top5] = (leaderboard[4] as dynamic).v;
                AchievementData[ArType.Top3] = (leaderboard[2] as dynamic).v;
            }

            if (AchievementData.Values.Any(x => $"{x}".Contains("."))) // Check is double bullshit
            {
                AchievementData = AchievementData.ToDictionary(x => x.Key, x => Convert(Math.Round(ToDouble(x.Value), 1, MidpointRounding.ToZero), valueType));
            }
            else
            {
                AchievementData = AchievementData.ToDictionary(x => x.Key, x => (dynamic)Math.Abs(int.Parse($"{x.Value}")));
            }

            if (rankType == rt.PERCENTILELAX || rankType == rt.PERCENTILEMLAX)
                AchievementData = AchievementData.Concat(new Dictionary<ArType, dynamic>() {
                        { ArType.Silver, "Any" },
                    }).ToDictionary();

            else if (rankType == rt.PERCENTILEVLAX)
                AchievementData = AchievementData.Concat(new Dictionary<ArType, dynamic>() {
                        { ArType.Gold, "Any" },
                    }).ToDictionary();
        }

        return new AchievementData
        {
            id = achievement.k,
            name = achievement.name,
            description = achievement.desc,

            arType = arType,

            rankType = (rt)ToInt(achievement.rt),
            valueType = (vt)ToInt(achievement.vt),

            cutoffs = AchievementData.OrderBy(x=> x.Key).ToDictionary(),
        };
    }

    public static string Convert(dynamic value, vt valueType)
    {
        value = Math.Round(Math.Abs(decimal.Parse($"{value}")), 1);

        if (valueType == vt.TIME_INV || valueType == vt.TIME)
        {
            return $"{(int)value / 60000}:{(int)value / 1000 % 60:00}:{(int)value % 1000:000}";
        }
        if (valueType == vt.FLOOR) return $"{value}m";

        return value.ToString();
    }
} // done
public enum Rank
{
    Xplus,
    X,
    U,
    SS,
    Splus,
    S,
    Sminus,
    Aplus,
    A,
    Aminus,
    Bplus,
    B,
    Bminus,
    Cplus,
    C,
    Cminus,
    Dplus,
    D,
    Z
}
public enum ArType
{
    Top3,
    Top5,
    Top10,
    Top25,
    Top50,
    Top100,
    Issued,
    Diamond,
    Platinum,
    Gold,
    Silver,
    Bronze,

    None
}
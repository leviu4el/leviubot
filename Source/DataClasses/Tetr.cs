using Discord;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Text.Json;
using static Source.DataClasses.Achievement;
using static System.Net.Mime.MediaTypeNames;

namespace Source.DataClasses
{
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

        #region const 
        // this shoudn't be public?
        public static readonly double apmweight = 1;
        public static readonly double ppsweight = 45;
        public static readonly double vsweight = 0.444;
        public static readonly double appweight = 185;
        public static readonly double dssweight = 175;
        public static readonly double dspweight = 450;
        public static readonly double dsappweight = 140;
        public static readonly double vsapmweight = 60;
        public static readonly double ciweight = 1.25;
        public static readonly double geweight = 315;

        public static readonly double apmsrw = 0;
        public static readonly double ppssrw = 135;
        public static readonly double vssrw = 0;
        public static readonly double appsrw = 290;
        public static readonly double dsssrw = 0;
        public static readonly double dspsrw = 700;
        public static readonly double garbageeffisrw = 0;
        #endregion

        public NerdStats(double apm, double pps, double vs)
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

            this.app = app;
            this.dss = dss;
            this.dsp = dsp;
            this.wapp = wapp;

            this.dsapp = dsapp;
            this.vsapm = vsapm;

            this.ci = ci;
            this.ge = ge;

            this.opener = opener;
            this.plonk = plonk;
            this.stride = stride;
            this.infds = infds;
        }
    }

    public class TetrisUser
    {
        public string UUID { get; private set; }
        public UserInfo Info { get; private set; }
        public FourtyLines FourtyLines { get; private set; }
        public Blitz Blitz { get; private set; }
        public QuickPlay QuickPlay { get; private set; }
        public ExpertQuickPlay ExpertQuickPlay { get; private set; }
        public TetraLeague TetraLeague { get; private set; }
        public Zen Zen { get; private set; }
        public List<Achievement> Achievements { get; private set; }

        public bool spamThing { get; private set; }

        public TetrisUser(string? username = null, ulong? id = null,  bool spamthing = false)
        {
            username ??= id != null ? CheckDiscord((ulong)id) : null;
            if (username == null)
                throw new EmbedException(title: "User not found");

            UUID = username.ToLower();
            Info = new UserInfo();
            FourtyLines = new FourtyLines();
            Blitz = new Blitz(); 
            QuickPlay = new QuickPlay(); 
            ExpertQuickPlay = new ExpertQuickPlay(); 
            TetraLeague = new TetraLeague(); 
            Zen = new Zen();
            Achievements = new List<Achievement>();

            spamThing = spamthing;
        }
        public static string CheckDiscord(ulong discordId)
        {
            if (Source.Program.storedUsers.Any(x => x.discord_id == discordId))
            {
                return Source.Program.storedUsers.First(x => x.discord_id == discordId).tetris_id;
            }
            var jsonObject = Source.Program.GetResult($"https://ch.tetr.io/api/users/search/discord:{discordId}");

            if (!Convert.ToBoolean(jsonObject.success) || jsonObject.data == null)
                throw new EmbedException(
                    parameters: new List<ExceptionParameter>()
                    {
                        new ExceptionParameter(nameof(discordId),discordId.ToString()),
                        new ExceptionParameter(nameof(jsonObject.success),jsonObject.success),
                        new ExceptionParameter(nameof(jsonObject.data),jsonObject.data),
                    },
                    title: "Search by discord connection fails"
                );

            Source.Program.SaveUser(discordId, (string)jsonObject.data.user._id);
            return jsonObject.data.user._id;
        }
        public bool GetUserInfo()
        {
            var obj = Source.Program.GetResult($"https://ch.tetr.io/api/users/{UUID}", spamThing);
            if (obj.data == null) return false;
            var data = obj.data;
            Info.Username = data.username;
            Info.UUID = data._id;
            Info.Avatar = data.avatar_revision;
            Info.Banner = data.banner_revision;

            Info.Gametime = data.gametime;
            Info.Xp = data.xp;

            Info.DiscordId = data.connections.discord?.id;
            Info.Ar = data.ar;


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
                    if (data.ar_counts.TryGetValue(arString, out JToken token) && token.Type == JTokenType.Integer)
                    {
                        int value = token.ToObject<int>();
                        arCounts.Add(type, value);
                    }
                }
            }
            Info.ArCounts = arCounts;
            return true;
        }
        public void GetAll() {
            var jsonObject = Source.Program.GetResult($"https://ch.tetr.io/api/users/{UUID}/summaries", spamThing);
            FetchFourtyLines      (jsonObject.data["40l"]);
            FetchBlitz            (jsonObject.data["blitz"]);
            FetchQuickPlay        (jsonObject.data["zenith"]);
            FetchExpertQuickPlay  (jsonObject.data["zenithex"]);
            FetchTetraLeague      (jsonObject.data["league"]);
            FetchZen              (jsonObject.data["zen"]);
            FetchAchievements     (jsonObject.data["achievements"]);
        }
        public void GetFourtyLines() =>     FetchFourtyLines        (Source.Program.GetResult($"https://ch.tetr.io/api/users/{UUID}/summaries/40l", spamThing).data);
        public void GetBlitz() =>           FetchBlitz              (Source.Program.GetResult($"https://ch.tetr.io/api/users/{UUID}/summaries/blitz", spamThing).data);
        public void GetQuickPlay() =>       FetchQuickPlay          (Source.Program.GetResult($"https://ch.tetr.io/api/users/{UUID}/summaries/zenith", spamThing).data);
        public void GetExpertQuickPlay() => FetchExpertQuickPlay    (Source.Program.GetResult($"https://ch.tetr.io/api/users/{UUID}/summaries/zenithex", spamThing).data);
        public void GetTetraLeague() =>     FetchTetraLeague        (Source.Program.GetResult($"https://ch.tetr.io/api/users/{UUID}/summaries/league", spamThing).data);
        public void GetZen() =>             FetchZen                (Source.Program.GetResult($"https://ch.tetr.io/api/users/{UUID}/summaries/zen", spamThing).data);
        public void GetAchievements() =>    FetchAchievements       (Source.Program.GetResult($"https://ch.tetr.io/api/users/{UUID}/summaries/achievements", spamThing).data);


        private bool FetchFourtyLines(dynamic data)
        {
            if (data == null) return false;
            if (data.record == null) return false;
            FourtyLines.Rank = data.rank;

            var record = data.record;
            //info.username ??= record.user.username;
            //info.id ??= record.user.id;
            //info.avatar ??= record.user.avatar_revision;
            //info.banner ??= record.user.banner_revision;

            FourtyLines.pps = record.results.aggregatestats.pps;
            FourtyLines.Inputs = record.results.stats.inputs;
            FourtyLines.Holds = record.results.stats.holds;
            FourtyLines.PiecesPlaced = record.results.stats.piecesplaced;

            FourtyLines.FinalTime = record.results.stats.finaltime;
            return true;
        }
        private bool FetchBlitz(dynamic data)
        {
            if (data == null) return false;
            if (data.record == null) return false;
            Blitz.Rank = data.rank;

            var record = data.record;
            //info.username ??= record.user.username;
            //info.id ??= record.user.id;
            //info.avatar ??= record.user.avatar_revision;
            //info.banner ??= record.user.banner_revision;

            Blitz.pps = record.results.aggregatestats.pps;
            Blitz.Inputs = record.results.stats.inputs;
            Blitz.Holds = record.results.stats.holds;
            Blitz.PiecesPlaced = record.results.stats.piecesplaced;
            Blitz.Allclear = record.results.stats.clears.allclear;

            Blitz.Score = record.results.stats.score;

            return true;
        }
        private void FetchQuickPlay(dynamic data)
        {
            
        }
        private void FetchExpertQuickPlay(dynamic data)
        {
            
        }
        private bool FetchTetraLeague(dynamic data)
        {
            if (data == null) return false;
            TetraLeague.seasons = [SeasonData.From(data)]; // current season

            var past = data.past as JObject;
            foreach (var item in past)
            {
                TetraLeague.seasons.Add(SeasonData.From(item.Value as dynamic));
            }

            TetraLeague.seasons.First(x => x.Season == -1).Season = TetraLeague.seasons.Max(x => x.Season) + 1;  // current season
            TetraLeague.CurrentSeason = TetraLeague.seasons.First();
            return true;
        }
        private bool FetchZen(dynamic data)
        {
            if (data == null) return false;
            Zen.Level = data.level;
            Zen.Score = data.score;
            return true;
        }
        private bool FetchAchievements(dynamic data)
        {
            if (data == null) return false;
            var allAchievements = data as JToken;
            foreach (dynamic achievement in allAchievements)
            {
                Achievements.Add(Achievement.From(achievement));
            }
            return true;
        }
    }
    public class UserInfo
    {
        private string? _UUID;
        public string UUID
        {
            get => _UUID != null ? _UUID : throw new ArgumentNullException(nameof(UUID));
            set => _UUID = value;
        }

        private string? _Username;
        public string Username
        {
            get => _Username != null ? _Username : throw new ArgumentNullException(nameof(Username));
            set => _Username = value;
        }

        public ulong? DiscordId;

        private string? _Avatar;
        public string Avatar
        {
            get => _Avatar != null ? _Avatar : throw new ArgumentNullException(nameof(Avatar));
            set => _Avatar = value;
        }

        private string? _Banner;
        public string Banner
        {
            get => _Banner != null ? _Banner : throw new ArgumentNullException(nameof(Banner));
            set => _Banner = value;
        }

        private int? _Ar;
        public int Ar
        {
            get => _Ar != null ? (int)_Ar : throw new ArgumentNullException(nameof(Ar));
            set => _Ar = value;
        }

        public Dictionary<ArType, int> ArCounts = new Dictionary<ArType, int>();

        private double? _Gametime;
        public double Gametime
        {
            get => _Gametime != null ? (double)_Gametime : throw new ArgumentNullException(nameof(Gametime));
            set => _Gametime = value;
        }

        private double? _Xp;
        public double Xp
        {
            get => _Xp != null ? (double)_Xp : throw new ArgumentNullException(nameof(Xp));
            set => _Xp = value;
        }
    } // done
    public class FourtyLines
    {
        private int? _Rank;
        public int Rank
        {
            get => _Rank != null ? (int)_Rank : throw new ArgumentNullException(nameof(Rank));
            set => _Rank = value;
        }
        private double? _pps;
        public double pps
        {
            get => _pps != null ? (int)_pps : throw new ArgumentNullException(nameof(pps));
            set => _pps = value;
        }
        private int? _Inputs;
        public int Inputs
        {
            get => _Inputs != null ? (int)_Inputs : throw new ArgumentNullException(nameof(Inputs));
            set => _Inputs = value;
        }
        private int? _Holds;
        public int Holds
        {
            get => _Holds != null ? (int)_Holds : throw new ArgumentNullException(nameof(Holds));
            set => _Holds = value;
        }
        private int? _PiecesPlaced;
        public int PiecesPlaced
        {
            get => _PiecesPlaced != null ? (int)_PiecesPlaced : throw new ArgumentNullException(nameof(PiecesPlaced));
            set => _PiecesPlaced = value;
        }
        private double? _FinalTime;
        public double FinalTime
        {
            get => _FinalTime != null ? (double)_FinalTime : throw new ArgumentNullException(nameof(FinalTime));
            set => _FinalTime = value;
        }
    } // done
    public class Blitz
    {
        private int? _Rank;
        public int Rank
        {
            get => _Rank != null ? (int)_Rank : throw new ArgumentNullException(nameof(Rank));
            set => _Rank = value;
        }
        private double? _pps;
        public double pps
        {
            get => _pps != null ? (int)_pps : throw new ArgumentNullException(nameof(pps));
            set => _pps = value;
        }
        private int? _Inputs;
        public int Inputs
        {
            get => _Inputs != null ? (int)_Inputs : throw new ArgumentNullException(nameof(Inputs));
            set => _Inputs = value;
        }
        private int? _Holds;
        public int Holds
        {
            get => _Holds != null ? (int)_Holds : throw new ArgumentNullException(nameof(Holds));
            set => _Holds = value;
        }
        private int? _PiecesPlaced;
        public int PiecesPlaced
        {
            get => _PiecesPlaced != null ? (int)_PiecesPlaced : throw new ArgumentNullException(nameof(PiecesPlaced));
            set => _PiecesPlaced = value;
        }
        private int? _Score;
        public int Score
        {
            get => _Score != null ? (int)_Score : throw new ArgumentNullException(nameof(Score));
            set => _Score = value;
        }
        private int? _Allclear;
        public int Allclear
        {
            get => _Allclear != null ? (int)_Allclear : throw new ArgumentNullException(nameof(Allclear));
            set => _Allclear = value;
        }
    } // done
    public class Zen
    {
        public int? _Level { get; set; }
        public int Level
        {
            get => _Level != null ? (int)_Level : throw new ArgumentNullException(nameof(Level));
            set => _Level = value;
        }
        public int? _Score;
        public int Score
        {
            get => _Score != null ? (int)_Score : throw new ArgumentNullException(nameof(Score));
            set => _Score = value;
        }
    } // done
    public class TetraLeague
    {
        private SeasonData? _CurrentSeason;
        public SeasonData CurrentSeason
        {
            get => _CurrentSeason != null ? (SeasonData)_CurrentSeason : throw new ArgumentNullException(nameof(CurrentSeason));
            set => _CurrentSeason = value;
        }
        public List<SeasonData> seasons = new List<SeasonData>();
    } // done
    public class SeasonData
    {
        private int? _Season;
        public int Season
        {
            get => _Season != null ? (int)_Season : throw new ArgumentNullException(nameof(Season));
            set => _Season = value;
        }
        private int? _GamesPlayed;
        public int GamesPlayed
        {
            get => _GamesPlayed != null ? (int)_GamesPlayed : throw new ArgumentNullException(nameof(GamesPlayed));
            set => _GamesPlayed = value;
        }
        private int? _GamesWon;
        private int GamesWon
        {
            get => _GamesWon != null ? (int)_GamesWon : throw new ArgumentNullException(nameof(GamesWon));
            set => _GamesWon = value;
        }
        private double? _Glicko;
        public double Glicko
        {
            get => _Glicko != null ? (double)_Glicko : throw new ArgumentNullException(nameof(Glicko));
            set => _Glicko = value;
        }
        private double? _Rd;
        public double Rd
        {
            get => _Rd != null ? (double)_Rd : throw new ArgumentNullException(nameof(Rd));
            set => _Rd = value;
        }
        private double? _Tr;
        public double Tr
        {
            get => _Tr != null ? (int)_Tr : throw new ArgumentNullException(nameof(Tr));
            set => _Tr = value;
        }
        private LeagueRank? _Rank;
        public LeagueRank Rank
        {
            get => _Rank != null ? (LeagueRank)_Rank : throw new ArgumentNullException(nameof(Rank));
            set => _Rank = value;
        }
        private LeagueRank? _BestRank;
        public LeagueRank BestRank
        {
            get => _BestRank != null ? (LeagueRank)_BestRank : throw new ArgumentNullException(nameof(BestRank));
            set => _BestRank = value;
        }
        private double? _apm;
        public double apm
        {
            get => _apm != null ? (double)_apm : throw new ArgumentNullException(nameof(apm));
            set => _apm = value;
        }
        private double? _pps;
        public double pps
        {
            get => _pps != null ? (double)_pps : throw new ArgumentNullException(nameof(pps));
            set => _pps = value;
        }
        private double? _vs;
        public double vs
        {
            get => _vs != null ? (double)_vs : throw new ArgumentNullException(nameof(vs));
            set => _vs = value;
        }
        //public NerdStats? _NerdStats;
        public NerdStats NerdStats
        {
            get => Extensions.Require(_apm, _pps, _vs) ? new NerdStats((double)_apm, (double)_pps, (double)_vs) : throw new ArgumentNullException(nameof(NerdStats));
        }

        private int? _Standing;
        public int Standing
        {
            get => _Standing != null ? (int)_Standing : throw new ArgumentNullException(nameof(Standing));
            set => _Standing = value;
        }
        public static SeasonData From(dynamic data)
        {
            Dictionary<string, LeagueRank> ranks = new Dictionary<string, LeagueRank>()
            {
                { "x+", LeagueRank.Xplus },
                { "x", LeagueRank.X },
                { "u", LeagueRank.U },
                { "ss", LeagueRank.SS },
                { "s+", LeagueRank.Splus },
                { "s", LeagueRank.S },
                { "s-", LeagueRank.Sminus },
                { "a+", LeagueRank.Aplus },
                { "a", LeagueRank.A },
                { "a-", LeagueRank.Aminus },
                { "b+", LeagueRank.Bplus },
                { "b", LeagueRank.B },
                { "b-", LeagueRank.Bminus },
                { "c+", LeagueRank.Cplus },
                { "c", LeagueRank.C },
                { "c-", LeagueRank.Cminus },
                { "d+", LeagueRank.Dplus },
                { "d", LeagueRank.D },
            };

            var season = new SeasonData
            {
                Season = data.season ?? -1,
                GamesPlayed = data.gamesplayed,
                GamesWon = data.gameswon,
                Glicko = data.glicko,
                Rd = data.rd,
                Tr = data.tr,
                Rank =  ranks.TryGetValue($"{data.rank}", out LeagueRank rank) ? rank : LeagueRank.Z,
                BestRank = ranks.TryGetValue($"{data.bestrank}", out LeagueRank bestRank) ? bestRank : LeagueRank.Z,
                Standing = 
                    data.standing != null ? Convert.ToInt32(data.standing) 
                    : data.placement != null ? Convert.ToInt32(data.placement) 
                    : -1
            };

            if (data.apm != null) season.apm = data.apm;
            if (data.vs != null) season.vs = data.vs;
            if (data.pps != null) season.pps = data.pps;

            return season;
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
        public int Id;
        public string Name;
        public string Description;

        public dynamic Value;
        public dynamic MainValue;
        public ArType Rank;

        public int Pos;

        public rt Rt;
        public vt Vt;
        public art Art;

        public bool Unlocked;

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

            art arType = (art)achievement.art;
            rt rankType = (rt)achievement.rt;
            vt valueType = (vt)achievement.vt;

            ArType rank = ArType.None;

            int pos = achievement.pos ?? -1;


            if (achievement.rank != null) rank = arValues[int.Parse($"{achievement.rank}")];
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
                Id = achievement.k,
                Name = achievement.name,
                Description = achievement.desc,

                Value = valueType == vt.ISSUE
                    ? achievement.pos
                    : achievement.rank != null
                        ? AchievementData.Convert(achievement.v, valueType) 
                        : "locked",
                MainValue = achievement.v ??= achievement.pos,

                Art = arType,
                Vt = valueType,
                Rt = rankType,

                Pos = pos,

                Unlocked = achievement.rank != null,
                Rank = rank,
            };
        }
    } // done
    public class AchievementData
    {
        public int Id;
        public string Name;
        public string Description;

        public rt rankType;
        public vt valueType;
        public art arType;

        public Dictionary<ArType, dynamic> cutoffs { get; set; }

        public static AchievementData From(dynamic data)
        {
            var achievement = data.achievement;
            var leaderboard = data.leaderboard as JToken;
            var cutoffs = data.cutoffs;

            art arType = (art)achievement.art;
            rt rankType = (rt)achievement.rt;
            vt valueType = (vt)achievement.vt;


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

                AchievementData = AchievementData.ToDictionary(x => x.Key,
                    x => $"{x}".Contains(".")
                    ? Convert(Math.Round(decimal.Parse($"{x.Value}"), 1, MidpointRounding.ToZero), valueType)
                    : (dynamic)Math.Abs(int.Parse($"{x.Value}"))
                );

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
                Id = achievement.k,
                Name = achievement.name,
                Description = achievement.desc,

                arType = arType,

                rankType = (rt)achievement.rt,
                valueType = (vt)achievement.vt,

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
    public enum LeagueRank
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
}

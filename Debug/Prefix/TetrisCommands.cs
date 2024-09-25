using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using QuickChart;
using Source.DataClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Debug.Prefix
{
    [Summary("Tetris")]
    public class TetrisCommands : ModuleBase<SocketCommandContext>
    {
        #region psq sq vs
        [Command("psq")]
        public async Task psq(params string[] users)
        {
            List<TetrisUser> tetrisUsers = new List<TetrisUser>();
            string reply = "";
            if (users.Length == 0)
            {
                try
                {
                    TetrisUser tetrisUser = new TetrisUser(id: Context.User.Id);

                    tetrisUser.GetUserInfo();
                    tetrisUser.GetTetraLeague();

                    Extensions.Require(
                        tetrisUser.Info.Username,

                        tetrisUser.TetraLeague.CurrentSeason.apm,
                        tetrisUser.TetraLeague.CurrentSeason.pps,
                        tetrisUser.TetraLeague.CurrentSeason.vs
                    );
                    tetrisUsers.Add(tetrisUser);
                }
                catch (Exception ex) //Search by user failed or 
                {
                    reply += $"Search by discord failed +{ex.Message}";
                }
            }
            else
            {
                foreach (string user in users)
                {
                    try
                    {
                        var tetrisUser = new TetrisUser(user);
                        tetrisUser.GetUserInfo();
                        tetrisUser.GetTetraLeague();

                        Extensions.Require(
                            tetrisUser.Info.Username,

                            tetrisUser.TetraLeague.CurrentSeason.apm, 
                            tetrisUser.TetraLeague.CurrentSeason.pps,
                            tetrisUser.TetraLeague.CurrentSeason.vs
                        );
                        tetrisUsers.Add(tetrisUser);
                    }
                    catch (EmbedException ex) //Search by user failed or 
                    {
                        reply += $"{user} is not a valid user\n";
                    }
                    catch (ArgumentNullException ex) //Search by user failed or 
                    {
                        reply += $"{user} tetra league amp/vs/pps is null\n";
                    }
                }
            }


            if (tetrisUsers.Count > 0)
            {
                var embed = new EmbedBuilder
                {
                    Description = reply,
                    ImageUrl = $"{CreateTriangleChart(tetrisUsers, playstyle: true)}"
                };

                foreach (var User in tetrisUsers)
                {
                    var nerd = User.TetraLeague.CurrentSeason.NerdStats;

                    embed.AddField(User.Info.Username,
                    $"opener: {nerd.opener}\n" +
                    $"plonk: {nerd.plonk}\n" +
                    $"stride: {nerd.stride}\n" +
                    $"infds: {nerd.infds}", true);
                }

                await Context.Message.ReplyAsync(embed: embed.Build(), allowedMentions: AllowedMentions.None);
            }
            else await Context.Message.ReplyAsync($"{reply}", allowedMentions: AllowedMentions.None);
        }

        [Command("sq")]
        public async Task sq(params string[] users)
        {
            if (users.FirstOrDefault() == "help")
            {
                await ReplyAsync(
                    "`>sq` - Displays ATTACK, SPEED, DEFENSE and CHEESE of users in radar graph.\n" +
                    "**Usage** - `>sq [name] [name2, optional] [name3, optional]`..."
                );
                return;
            }

            List<TetrisUser> tetrisUsers = new List<TetrisUser>();
            string reply = "";
            if (users.Length == 0)
            {
                try
                {
                    TetrisUser tetrisUser = new TetrisUser(id: Context.User.Id);

                    tetrisUser.GetUserInfo();
                    tetrisUser.GetTetraLeague();

                    Extensions.Require(
                        tetrisUser.Info.Username,

                        tetrisUser.TetraLeague.CurrentSeason.apm,
                        tetrisUser.TetraLeague.CurrentSeason.pps,
                        tetrisUser.TetraLeague.CurrentSeason.vs
                    );
                    tetrisUsers.Add(tetrisUser);
                }
                catch (Exception ex) //Search by user failed or 
                {
                    reply += $"Search by discord failed +{ex.Message}";
                }
            }
            else
            {
                foreach (string user in users)
                {
                    try
                    {
                        var tetrisUser = new TetrisUser(user);
                        tetrisUser.GetUserInfo();
                        tetrisUser.GetTetraLeague();

                        Extensions.Require(
                            tetrisUser.Info.Username,

                            tetrisUser.TetraLeague.CurrentSeason.apm,
                            tetrisUser.TetraLeague.CurrentSeason.pps,
                            tetrisUser.TetraLeague.CurrentSeason.vs
                        );
                        tetrisUsers.Add(tetrisUser);
                    }
                    catch (EmbedException ex) //Search by user failed or 
                    {
                        reply += $"{user} is not a valid user\n";
                    }
                    catch (ArgumentNullException ex) //Search by user failed or 
                    {
                        reply += $"{user} tetra league amp/vs/pps is null\n";
                    }
                }
            }

            if (tetrisUsers.Count > 0)
            {

                var embed = new EmbedBuilder
                {
                    Description = reply,
                    ImageUrl = $"{CreateTriangleChart(tetrisUsers, playstyle: false)}"
                };

                foreach (var User in tetrisUsers)
                {
                    var nerd = User.TetraLeague.CurrentSeason.NerdStats;
                    embed.AddField(User.Info.Username,
                    $"attack: {Math.Round((decimal)(User.TetraLeague.CurrentSeason.apm / 60 * 0.4), 4)}\n" +
                    $"speed: {Math.Round((decimal)(User.TetraLeague.CurrentSeason.pps / 3.75), 4)}\n" +
                    $"defence: {Math.Round((decimal)(nerd.dss * 1.15), 4)}\n" +
                    $"cheese: {Math.Round((decimal)(nerd.ci / 110), 4)}", true);
                }

                //Your embed needs to be built before it is able to be sent
                await Context.Message.ReplyAsync(embed: embed.Build(), allowedMentions: AllowedMentions.None);
            }
            else await Context.Message.ReplyAsync($"{reply}", allowedMentions: AllowedMentions.None);
        }

        [Command("vs")]
        public async Task vs(params string[] users)
        {
            if (users.FirstOrDefault() == "help")
            {
                await ReplyAsync(
                    "`>psq` - Same thing as sq, but instead of each end being ATTACK, SPEED, DEFENSE and CHEESE, you have OPENER, STRIDE, PLONK and INF DS.\n" +
                    "**Usage** - `>psq [name] [name2, optional] [name3, optional]`..."
                );
                return;
            }
            List<TetrisUser> tetrisUsers = new List<TetrisUser>();
            string reply = "";
            if (users.Length == 0)
            {
                try
                {
                    TetrisUser tetrisUser = new TetrisUser(id: Context.User.Id);

                    tetrisUser.GetUserInfo();
                    tetrisUser.GetTetraLeague();

                    Extensions.Require(
                        tetrisUser.Info.Username,

                        tetrisUser.TetraLeague.CurrentSeason.apm,
                        tetrisUser.TetraLeague.CurrentSeason.pps,
                        tetrisUser.TetraLeague.CurrentSeason.vs
                    );
                    tetrisUsers.Add(tetrisUser);
                }
                catch (Exception ex) //Search by user failed or 
                {
                    reply += $"Search by discord failed +{ex.Message}";
                }
            }
            else
            {
                foreach (string user in users)
                {
                    try
                    {
                        var tetrisUser = new TetrisUser(user);
                        tetrisUser.GetUserInfo();
                        tetrisUser.GetTetraLeague();

                        Extensions.Require(
                            tetrisUser.Info.Username,

                            tetrisUser.TetraLeague.CurrentSeason.apm,
                            tetrisUser.TetraLeague.CurrentSeason.pps,
                            tetrisUser.TetraLeague.CurrentSeason.vs
                        );
                        tetrisUsers.Add(tetrisUser);
                    }
                    catch (EmbedException ex) //Search by user failed or 
                    {
                        reply += $"{user} is not a valid user\n";
                    }
                    catch (ArgumentNullException ex) //Search by user failed or 
                    {
                        reply += $"{user} tetra league amp/vs/pps is null\n";
                    }
                }
            }

            if (tetrisUsers.Count > 0)
            {
                var embed = new EmbedBuilder
                {
                    Description = reply,
                    ImageUrl = $"{CreateVersusChart(tetrisUsers)}"
                };
                //'APM', 'PPS', 'VS', 'APP', 'DS/Second', 'DS/Piece', 'APP+DS/Piece', 'VS/APM', 'Cheese Index', 'Garbage Effi.'
                foreach (var user in tetrisUsers)
                {
                    var nerd = user.TetraLeague.CurrentSeason.NerdStats;
                    embed.AddField(user.Info.Username,
                    $"apm: {Math.Round((decimal)user.TetraLeague.CurrentSeason.apm, 4)}\n" +
                    $"pps: {Math.Round((decimal)user.TetraLeague.CurrentSeason.pps, 4)}\n" +
                    $"vs: {Math.Round((decimal)user.TetraLeague.CurrentSeason.vs, 4)}\n" +
                    $"app: {Math.Round((decimal)nerd.app, 4)}\n" +
                    $"ds/second: {Math.Round((decimal)nerd.dss, 4)}\n" +
                    $"ds/piece: {Math.Round((decimal)nerd.dsp, 4)}\n" +
                    $"app+ds/piece: {Math.Round((decimal)nerd.dsapp, 4)}\n" +
                    $"vs/apm: {Math.Round((decimal)nerd.vsapm, 4)}\n" +
                    $"cheese index: {Math.Round((decimal)nerd.ci, 4)}\n" +
                    $"garbage effi: {Math.Round((decimal)nerd.ge, 4)}"
                    , true);

                }

                //Your embed needs to be built before it is able to be sent
                await Context.Message.ReplyAsync(embed: embed.Build(), allowedMentions: AllowedMentions.None);
            }
            else await Context.Message.ReplyAsync($"{reply}", allowedMentions: AllowedMentions.None);
        }

        private static string CreateTriangleChart(List<TetrisUser> tetrisUsers, bool playstyle)
        {
            Chart qc = new Chart();
            string config = ChartData.TriangleConfig;

            config = Regex.Replace(config, @"labels: \[\],",
                playstyle == false ? "labels: ['ATTACK', 'SPEED', 'DEFENSE', 'CHEESE'],"
                                        : "labels: ['OPENER', 'STRIDE', 'INF DS', 'PLONK'],");
            var backgroundColors = new List<string> { "rgba(204, 253, 232, 0.65)", "rgba(48, 186, 255, 0.65)", "rgba(240, 86, 127, 0.65)", "rgba(8, 209, 109, 0.65)", "rgba(237, 156, 17, 0.65)" };
            var borderColors = new List<string> { "rgba(75, 118, 191, 1)", "rgba(204, 33, 201, 1)", "rgba(250, 5, 70, 1)", "rgba(28, 232, 130, 1)", "rgba(250, 177, 42, 1)" };

            string datasets = "";
            int count = 0;
            tetrisUsers.ForEach(t =>
            {
                string temp =
                @"{
                    label: [],
                    data: []
                },";
                if (count < 5)
                {
                    temp =
                    @"{
                        backgroundColor: [],
                        borderColor: [],
                        label: [],
                        data: []
                    },";

                    temp = Regex.Replace(temp, @"backgroundColor: \[\],", $"backgroundColor: '{backgroundColors[count]}',");
                    temp = Regex.Replace(temp, @"borderColor: \[\],", $"borderColor: '{borderColors[count]}',");
                }

                var nerd = t.TetraLeague.CurrentSeason.NerdStats;
                temp = Regex.Replace(temp, @"data: \[\]",
                    playstyle == false ? $"data: [{(t.TetraLeague.CurrentSeason.apm / 60 * 0.4).ToString().Replace(",", ".")}, {(t.TetraLeague.CurrentSeason.pps / 3.75).ToString().Replace(",", ".")}, {(nerd.dss * 1.15).ToString().Replace(",", ".")}, {(nerd.ci / 110).ToString().Replace(",", ".")}]"
                                            : $"data: [{nerd.opener.ToString().Replace(",", ".")}, {nerd.stride.ToString().Replace(",", ".")}, {nerd.infds.ToString().Replace(",", ".")}, {nerd.plonk.ToString().Replace(",", ".")}]");

                temp = Regex.Replace(temp, @"label: \[\],", $"label: '{t.Info.Username}',");


                datasets += temp;
                count++;
            });
            config = Regex.Replace(config, @"datasets: \[\]", $"datasets: [{datasets}]");

            config = Regex.Replace(config, @"display: \[\],",
            tetrisUsers.Count > 20 ? "display: false," : "display: true,");



            config = Regex.Replace(config, @"max: \[\],",
            playstyle == false ? "max: 1.2,"
                                    : "max: 1.5,");

            config = Regex.Replace(config, @"stepSize: \[\],",
            playstyle == false ? "stepSize: 0.2,"
                                    : "stepSize: 0.25,");

            qc.Config = config;
            return qc.GetShortUrl();
        }
        private static string CreateVersusChart(List<TetrisUser> tetrisUsers)
        {
            Chart qc = new Chart();
            string config = ChartData.VersusConfig;

            var backgroundColors = new List<string> { "rgba(204, 253, 232, 0.65)", "rgba(48, 186, 255, 0.65)", "rgba(240, 86, 127, 0.65)", "rgba(8, 209, 109, 0.65)", "rgba(237, 156, 17, 0.65)" };
            var borderColors = new List<string> { "rgba(75, 118, 191, 1)", "rgba(204, 33, 201, 1)", "rgba(250, 5, 70, 1)", "rgba(28, 232, 130, 1)", "rgba(250, 177, 42, 1)" };

            string datasets = "";
            int count = 0;
            tetrisUsers.ForEach(t =>
            {
                string temp =
                @"{
                    label: [],
                    data: []
                },";
                if (count < 5)
                {
                    temp =
                    @"{
                        backgroundColor: [],
                        borderColor: [],
                        label: [],
                        data: []
                    },";

                    temp = Regex.Replace(temp, @"backgroundColor: \[\],", $"backgroundColor: '{backgroundColors[count]}',");
                    temp = Regex.Replace(temp, @"borderColor: \[\],", $"borderColor: '{borderColors[count]}',");
                }
                var nerd = t.TetraLeague.CurrentSeason.NerdStats;
                temp = Regex.Replace(temp, @"data: \[\]", $"data: [" +
                $"{Math.Round((decimal)(t.TetraLeague.CurrentSeason.apm * NerdStats.apmweight), 4).ToString().Replace(",", ".")}," +
                $"{Math.Round((decimal)(t.TetraLeague.CurrentSeason.pps * NerdStats.ppsweight), 4).ToString().Replace(",", ".")}," +
                $"{Math.Round((decimal)(t.TetraLeague.CurrentSeason.vs * NerdStats.vsweight), 4).ToString().Replace(",", ".")}," +
                $"{Math.Round((decimal)(nerd.app * NerdStats.appweight), 4).ToString().Replace(",", ".")}," +
                $"{Math.Round((decimal)(nerd.dss * NerdStats.dssweight), 4).ToString().Replace(",", ".")}," +
                $"{Math.Round((decimal)(nerd.dsp * NerdStats.dspweight), 4).ToString().Replace(",", ".")}," +
                $"{Math.Round((decimal)(nerd.dsapp * NerdStats.dsappweight), 4).ToString().Replace(",", ".")}," +
                $"{Math.Round((decimal)(nerd.vsapm * NerdStats.vsapmweight), 4).ToString().Replace(",", ".")}," +
                $"{Math.Round((decimal)(nerd.ci * NerdStats.ciweight), 4).ToString().Replace(",", ".")}," +
                $"{Math.Round((decimal)(nerd.ge * NerdStats.geweight), 4).ToString().Replace(",", ".")}]");

                temp = Regex.Replace(temp, @"label: \[\],", $"label: '{t.Info.Username}',");

                datasets += temp;
                count++;
            });
            config = Regex.Replace(config, @"datasets: \[\]", $"datasets: [{datasets}]");

            config = Regex.Replace(config, @"display: \[\],",
            tetrisUsers.Count > 20 ? "display: false," : "display: true,");
            qc.Config = config;
            return qc.GetShortUrl();
        }
        #endregion

    }
}

using System.Text.Json.Serialization;

namespace api.model;

public class TetrisUser
{
    [JsonPropertyName("uuid")] public string Uuid { get; set; }
    
    [JsonPropertyName("info")] public Info Info { get; set; } = new Info();
    [JsonPropertyName("league")] public League League { get; set; } = new League();
    [JsonPropertyName("zen")] public Zen Zen { get; set; } = new Zen();
    [JsonPropertyName("fortyLines")] public FortyLines FortyLines { get; set; } = new FortyLines();
    [JsonPropertyName("blitz")] public Blitz Blitz { get; set; } = new Blitz();
}

public class Info
{
    [JsonPropertyName("username")] public string? Username { get; set; }
}
public class League
{
    [JsonPropertyName("gamesplayed")] public int? GamesPlayed { get; set; }
    [JsonPropertyName("gameswon")] public int? GamesWon { get; set; }
    [JsonPropertyName("glicko")] public double? Glicko { get; set; }
    [JsonPropertyName("rd")] public double? Rd { get; set; }
    [JsonPropertyName("tr")] public double? Tr { get; set; }
    [JsonPropertyName("gxe")] public double? Gxe { get; set; }
    [JsonPropertyName("rank")] public string? Rank { get; set; }
    [JsonPropertyName("bestrank")] public string? BestRank { get; set; }
    [JsonPropertyName("apm")] public double? Apm { get; set; }
    [JsonPropertyName("pps")] public double? Pps { get; set; }
    [JsonPropertyName("vs")] public double? Vs { get; set; }
}

public class Valentine
{
    [JsonPropertyName("username")] public string Username { get; set; }
    [JsonPropertyName("uuid")] public string Uuid { get; set; }
    [JsonPropertyName("valentineUsername")] public string? ValentineUsername { get; set; }
    [JsonPropertyName("valentineUuid")] public string? ValentineUuid { get; set; }
}

public class Zen
{
    [JsonPropertyName("level")] public int? Level { get; set; }
}
public class FortyLines
{
    [JsonPropertyName("finaltime")] public Double? Finaltime { get; set; }
}

public class Blitz
{
    [JsonPropertyName("score")] public int? Score { get; set; }
}
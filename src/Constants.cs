namespace BrawlhallaReplayReader
{
    public static class Constants
    {
        public static readonly List<string> PlayerHeaders = new List<string> { "time", "name", "placement", "deaths", "hero", "map", "playerCount", "gameLength", "players", "version", "checksum", "isTeam", "teammates" };

        public static readonly List<string> GameHeaders = new List<string> { "time", "map", "version", "checksum", "players", "heroes", "deaths", "playerCount", "winner", "gameLength", "isTeam", "teams" };

        public static readonly Dictionary<int, string> Heroes = new Dictionary<int, string>{
            {3, "bodvar" },
            { 5, "orion" },
            { 6, "lord_vrax" },
            { 8, "queen_nai" },
            { 10, "hatori" },
            { 11, "sir_roland" },
            { 13, "thatch" },
            { 16, "teros" },
            { 17, "red_raptor" },
            { 19, "brynn" },
            { 20, "asuri" },
            { 22, "ulgrim" },
            { 23, "azoth" },
            { 27, "loki" },
            { 30, "val" },
            { 31, "ragnir" },
            { 34, "nix" },
            { 35, "mordex" }, // tai_lung
            { 37, "artemis" },
            { 40, "xull" },
            { 41, "isaiah" },
            { 42, "kaya" }, // pearl
            { 43, "jiro" },
            { 44, "lin_fei" },
            { 45, "zariel" },
            { 46, "rayman" },
            { 50, "petra" },
            { 51, "vector" },
            { 55, "mako" },
            { 56, "magyar" },
            { 57, "reno" },
            { 58, "munin" },
            { 59, "arcadia" },
            { 60, "ezio" },
            { 61, "seven" },
            { 62, "thea" },
            { 63, "tezca" },
            { 66, "king_zuva" },
            { 67, "priya" }
        };
    }
}
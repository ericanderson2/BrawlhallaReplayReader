namespace BrawlhallaReplayReader
{
    /// <summary>
    /// Represents general information about a game.
    /// </summary>
    public class GameInfo
    {
        public DateTimeOffset StartTime { get; set; }

        public string Map { get; set; } = String.Empty;

        public float Version { get; set; }

        public uint Checksum { get; set; }

        public int GameLength { get; set; }

        public List<string> Players { get; set; } = new List<string>();

        public List<int> Deaths { get; set; } = new List<int>();

        public List<string> Heroes { get; set; } = new List<string>();

        public bool IsTeam { get; set; }

        public List<List<string>> Teams { get; set; } = new List<List<string>>();

        public string ToCsvString()
        {
            string[] properties = new string[Constants.GameHeaders.Count];
            properties[Constants.GameHeaders.IndexOf("time")] = StartTime.ToString("s", System.Globalization.CultureInfo.InvariantCulture);
            properties[Constants.GameHeaders.IndexOf("map")] = Map;
            properties[Constants.GameHeaders.IndexOf("version")] = Version.ToString();
            properties[Constants.GameHeaders.IndexOf("checksum")] = Checksum.ToString();
            properties[Constants.GameHeaders.IndexOf("players")] = $"\"{String.Join(";", Players)}\"";
            properties[Constants.GameHeaders.IndexOf("heroes")] = $"\"{String.Join(";", Heroes)}\"";
            properties[Constants.GameHeaders.IndexOf("deaths")] = $"\"{String.Join(";", Deaths)}\"";
            properties[Constants.GameHeaders.IndexOf("playerCount")] = Players.Count.ToString();
            properties[Constants.GameHeaders.IndexOf("winner")] = Players[0];
            if (IsTeam)
            {
                string winningTeam = String.Join(';', Teams.Find((team) => team.Contains(Players[0])));
                properties[Constants.GameHeaders.IndexOf("winner")] = $"\"{winningTeam}\"";
            }
            properties[Constants.GameHeaders.IndexOf("gameLength")] = GameLength.ToString();
            properties[Constants.GameHeaders.IndexOf("isTeam")] = IsTeam.ToString();
            properties[Constants.GameHeaders.IndexOf("teams")] = $"\"{String.Join(';', Teams.Select(team => $"[{String.Join(";", team)}]"))}\"";
            return String.Join(",", properties);
        }
    }
}
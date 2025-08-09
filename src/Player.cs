namespace BrawlhallaReplayReader
{
    public class PlayerProfile
    {
        public string Name { get; set; } = String.Empty;

        public int Placement { get; set; }

        public string Hero { get; set; } = String.Empty;

        public int Deaths { get; set; }

        public int Team { get; set; }

        public List<string> TeamMates { get; set; } = new List<string>();

        public GameInfo GameInfo = new GameInfo();

        public string ToCsvString()
        {
            string[] properties = new string[Constants.PlayerHeaders.Count];
            properties[Constants.PlayerHeaders.IndexOf("time")] = GameInfo.StartTime.ToString("s", System.Globalization.CultureInfo.InvariantCulture);
            properties[Constants.PlayerHeaders.IndexOf("name")] = Name;
            properties[Constants.PlayerHeaders.IndexOf("placement")] = Placement.ToString();
            properties[Constants.PlayerHeaders.IndexOf("deaths")] = Deaths.ToString();
            properties[Constants.PlayerHeaders.IndexOf("hero")] = Hero;
            properties[Constants.PlayerHeaders.IndexOf("map")] = GameInfo.Map;
            properties[Constants.PlayerHeaders.IndexOf("playerCount")] = GameInfo.Players.Count.ToString();
            properties[Constants.PlayerHeaders.IndexOf("gameLength")] = GameInfo.GameLength.ToString();
            properties[Constants.PlayerHeaders.IndexOf("players")] = $"\"{String.Join(";", GameInfo.Players)}\"";
            properties[Constants.PlayerHeaders.IndexOf("version")] = GameInfo.Version.ToString();
            properties[Constants.PlayerHeaders.IndexOf("checksum")] = GameInfo.Checksum.ToString();
            properties[Constants.PlayerHeaders.IndexOf("isTeam")] = GameInfo.IsTeam.ToString();
            properties[Constants.PlayerHeaders.IndexOf("teammates")] = $"\"{String.Join(";", TeamMates)}\"";
            return String.Join(",", properties);
        }
    }
}
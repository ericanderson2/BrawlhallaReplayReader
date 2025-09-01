using Newtonsoft.Json;

namespace BrawlhallaReplayReader
{
    public class Player
    {
        public string Name { get; set; } = String.Empty;

        public int Placement { get; set; }

        public string Hero { get; set; } = String.Empty;

        public int Deaths { get; set; }

        public DateTimeOffset JoinTime { get; set; }

        public List<string> TeamMates { get; set; } = new List<string>();
    }

    public class GameReplay
    {
        public DateTimeOffset StartTime { get; set; }

        public string Map { get; set; } = String.Empty;

        public int ReplayVersion { get; set; }

        public float GameVersion { get; set; }

        public int GameLength { get; set; }

        public bool IsTeam { get; set; }

        // For strikeout games
        // This replay class only supports one hero per player, so the hero data is inaccurate for strikeout games
        public bool MultipleHeroesUsed { get; set; }

        public List<Player> Players { get; set; } = new List<Player>();

        public List<List<string>> Teams { get; set; } = new List<List<string>>();

        public GameReplay(v8_00.ReplayData replayData, float gameVersion = 0)
        {
            ReplayVersion = replayData.Version;
            GameVersion = gameVersion;
            GameLength = replayData.Length;
            Map = Helpers.GetLevelName(replayData.LevelId);
            MultipleHeroesUsed = false;
            IsTeam = new HashSet<int>(replayData.Results.Values).Count != replayData.Results.Values.Count();
            Dictionary<int, List<string>> teamMap = new Dictionary<int, List<string>>();
            if (IsTeam)
            {
                replayData.Results.Keys.ToList().ForEach(playerId =>
                {
                    var placement = replayData.Results[playerId];
                    if (!teamMap.ContainsKey(placement))
                    {
                        teamMap[placement] = new List<string>();
                    }
                    string playerName = replayData.Entities.Find(e => e.Id == playerId)?.Name ?? playerId.ToString();
                    teamMap[placement].Add(playerName);
                });
                Teams = teamMap.Values.ToList();
            }

            replayData.Entities.ForEach((entity) =>
            {
                if (!replayData.Results.ContainsKey(entity.Id))
                {
                    throw new Exception($"No placement results found for player {entity.Name}. The replay owner likely left while the match was in progress.");
                }

                Players.Add(new Player()
                {
                    Name = entity.Name,
                    Hero = Helpers.GetHeroName(entity.Data.Heroes[0].HeroId),
                    Deaths = replayData.Deaths.Count(d => d.EntityId == entity.Id),
                    Placement = replayData.Results[entity.Id],
                    JoinTime = DateTimeOffset.FromUnixTimeSeconds(entity.Data.Unknown2),
                    TeamMates = IsTeam ? teamMap[replayData.Results[entity.Id]] : []
                });

                if (entity.Data.Heroes.Count > 1)
                {
                    MultipleHeroesUsed = true;
                }
            });

            InitStartTime();
            SortPlayersByPlacement();
        }

        public GameReplay(v8_06.Replay replayData, float gameVersion = 0)
        {
            ReplayVersion = (int)replayData.Version;
            GameVersion = gameVersion;
            GameLength = (int)replayData.Length;
            Map = Helpers.GetLevelName((int)replayData.LevelID);
            MultipleHeroesUsed = false;
            IsTeam = new HashSet<int>(replayData.Results.Values).Count != replayData.Results.Values.Count();
            Dictionary<int, List<string>> teamMap = new Dictionary<int, List<string>>();

            var entities = replayData.Entities.ToList();
            if (IsTeam)
            {
                replayData.Results.Keys.ToList().ForEach(playerId =>
                {
                    var placement = replayData.Results[playerId];
                    if (!teamMap.ContainsKey(placement))
                    {
                        teamMap[placement] = new List<string>();
                    }
                    string playerName = entities.Find(e => e.EntityID == playerId).Name ?? playerId.ToString();
                    teamMap[placement].Add(playerName);
                });
                Teams = teamMap.Values.ToList();
            }

            entities.ForEach((entity) =>
            {
                if (!replayData.Results.ContainsKey(entity.EntityID))
                {
                    throw new Exception($"No placement results found for player {entity.Name}. The replay owner likely left while the match was in progress.");
                }

                Players.Add(new Player()
                {
                    Name = entity.Name,
                    Hero = Helpers.GetHeroName((int)entity.Player.Heroes[0].HeroID),
                    Deaths = replayData.Deaths.Count(d => d.EntityID == entity.EntityID),
                    Placement = replayData.Results[entity.EntityID],
                    JoinTime = DateTimeOffset.FromUnixTimeSeconds(entity.Player.ConnectionTime),
                    TeamMates = IsTeam ? teamMap[replayData.Results[entity.EntityID]] : []
                });

                if (entity.Player.Heroes.Count > 1)
                {
                    MultipleHeroesUsed = true;
                }
            });

            InitStartTime();
            SortPlayersByPlacement();
        }

        public GameReplay(v8_07.Replay replayData, float gameVersion = 0)
        {
            ReplayVersion = (int)replayData.Version;
            GameVersion = gameVersion;
            GameLength = (int)replayData.Length;
            Map = Helpers.GetLevelName((int)replayData.LevelID);
            MultipleHeroesUsed = false;
            IsTeam = new HashSet<int>(replayData.Results.Values).Count != replayData.Results.Values.Count();
            Dictionary<int, List<string>> teamMap = new Dictionary<int, List<string>>();

            var entities = replayData.Entities.ToList();
            if (IsTeam)
            {
                replayData.Results.Keys.ToList().ForEach(playerId =>
                {
                    var placement = replayData.Results[playerId];
                    if (!teamMap.ContainsKey(placement))
                    {
                        teamMap[placement] = new List<string>();
                    }
                    string playerName = entities.Find(e => e.EntityID == playerId).Name ?? playerId.ToString();
                    teamMap[placement].Add(playerName);
                });
                Teams = teamMap.Values.ToList();
            }

            entities.ForEach((entity) =>
            {
                if (!replayData.Results.ContainsKey(entity.EntityID))
                {
                    throw new Exception($"No placement results found for player {entity.Name}. The replay owner likely left while the match was in progress.");
                }

                Players.Add(new Player()
                {
                    Name = entity.Name,
                    Hero = Helpers.GetHeroName((int)entity.Player.Heroes[0].HeroID),
                    Deaths = replayData.Deaths.Count(d => d.EntityID == entity.EntityID),
                    Placement = replayData.Results[entity.EntityID],
                    JoinTime = DateTimeOffset.FromUnixTimeSeconds(entity.Player.ConnectionTime),
                    TeamMates = IsTeam ? teamMap[replayData.Results[entity.EntityID]] : []
                });

                if (entity.Player.Heroes.Count > 1)
                {
                    MultipleHeroesUsed = true;
                }
            });

            InitStartTime();
            SortPlayersByPlacement();
        }

        public GameReplay(v9_01.Replay replayData, float gameVersion = 0)
        {
            ReplayVersion = (int)replayData.Version;
            GameVersion = gameVersion;
            GameLength = (int)replayData.Length;
            Map = Helpers.GetLevelName((int)replayData.LevelID);
            MultipleHeroesUsed = false;
            IsTeam = new HashSet<int>(replayData.Results.Values).Count != replayData.Results.Values.Count();
            Dictionary<int, List<string>> teamMap = new Dictionary<int, List<string>>();

            var entities = replayData.Entities.ToList();
            if (IsTeam)
            {
                replayData.Results.Keys.ToList().ForEach(playerId =>
                {
                    var placement = replayData.Results[playerId];
                    if (!teamMap.ContainsKey(placement))
                    {
                        teamMap[placement] = new List<string>();
                    }
                    string playerName = entities.Find(e => e.EntityID == playerId).Name ?? playerId.ToString();
                    teamMap[placement].Add(playerName);
                });
                Teams = teamMap.Values.ToList();
            }

            entities.ForEach((entity) =>
            {
                if (!replayData.Results.ContainsKey(entity.EntityID))
                {
                    throw new Exception($"No placement results found for player {entity.Name}. The replay owner likely left while the match was in progress.");
                }

                Players.Add(new Player()
                {
                    Name = entity.Name,
                    Hero = Helpers.GetHeroName((int)entity.Player.Heroes[0].HeroID),
                    Deaths = replayData.Deaths.Count(d => d.EntityID == entity.EntityID),
                    Placement = replayData.Results[entity.EntityID],
                    JoinTime = DateTimeOffset.FromUnixTimeSeconds(entity.Player.ConnectionTime),
                    TeamMates = IsTeam ? teamMap[replayData.Results[entity.EntityID]] : []
                });

                if (entity.Player.Heroes.Count > 1)
                {
                    MultipleHeroesUsed = true;
                }
            });

            InitStartTime();
            SortPlayersByPlacement();
        }

        public GameReplay(v9_08.Replay replayData, float gameVersion = 0)
        {
            ReplayVersion = (int)replayData.Version;
            GameVersion = gameVersion;
            GameLength = (int)replayData.Length;
            Map = Helpers.GetLevelName((int)replayData.LevelID);
            MultipleHeroesUsed = false;
            IsTeam = new HashSet<int>(replayData.Results.Values).Count != replayData.Results.Values.Count();
            Dictionary<int, List<string>> teamMap = new Dictionary<int, List<string>>();

            var entities = replayData.Entities.ToList();
            if (IsTeam)
            {
                replayData.Results.Keys.ToList().ForEach(playerId =>
                {
                    var placement = replayData.Results[playerId];
                    if (!teamMap.ContainsKey(placement))
                    {
                        teamMap[placement] = new List<string>();
                    }
                    string playerName = entities.Find(e => e.EntityID == playerId).Name ?? playerId.ToString();
                    teamMap[placement].Add(playerName);
                });
                Teams = teamMap.Values.ToList();
            }

            entities.ForEach((entity) =>
            {
                if (!replayData.Results.ContainsKey(entity.EntityID))
                {
                    throw new Exception($"No placement results found for player {entity.Name}. The replay owner likely left while the match was in progress.");
                }

                Players.Add(new Player()
                {
                    Name = entity.Name,
                    Hero = Helpers.GetHeroName((int)entity.Player.Heroes[0].HeroID),
                    Deaths = replayData.Deaths.Count(d => d.EntityID == entity.EntityID),
                    Placement = replayData.Results[entity.EntityID],
                    JoinTime = DateTimeOffset.FromUnixTimeSeconds(entity.Player.ConnectionTime),
                    TeamMates = IsTeam ? teamMap[replayData.Results[entity.EntityID]] : []
                });

                if (entity.Player.Heroes.Count > 1)
                {
                    MultipleHeroesUsed = true;
                }
            });

            InitStartTime();
            SortPlayersByPlacement();
        }

        public GameReplay(v9_11.Replay replayData, float gameVersion = 0)
        {
            ReplayVersion = (int)replayData.Version;
            GameVersion = gameVersion;
            GameLength = (int)replayData.Length;
            Map = Helpers.GetLevelName((int)replayData.LevelID);
            MultipleHeroesUsed = false;
            IsTeam = new HashSet<int>(replayData.Results.Values).Count != replayData.Results.Values.Count();
            Dictionary<int, List<string>> teamMap = new Dictionary<int, List<string>>();

            var entities = replayData.Entities.ToList();
            if (IsTeam)
            {
                replayData.Results.Keys.ToList().ForEach(playerId =>
                {
                    var placement = replayData.Results[playerId];
                    if (!teamMap.ContainsKey(placement))
                    {
                        teamMap[placement] = new List<string>();
                    }
                    string playerName = entities.Find(e => e.EntityID == playerId).Name ?? playerId.ToString();
                    teamMap[placement].Add(playerName);
                });
                Teams = teamMap.Values.ToList();
            }

            entities.ForEach((entity) =>
            {
                if (!replayData.Results.ContainsKey(entity.EntityID))
                {
                    throw new Exception($"No placement results found for player {entity.Name}. The replay owner likely left while the match was in progress.");
                }

                Players.Add(new Player()
                {
                    Name = entity.Name,
                    Hero = Helpers.GetHeroName((int)entity.Player.Heroes[0].HeroID),
                    Deaths = replayData.Deaths.Count(d => d.EntityID == entity.EntityID),
                    Placement = replayData.Results[entity.EntityID],
                    JoinTime = DateTimeOffset.FromUnixTimeSeconds(entity.Player.ConnectionTime),
                    TeamMates = IsTeam ? teamMap[replayData.Results[entity.EntityID]] : []
                });

                if (entity.Player.Heroes.Count > 1)
                {
                    MultipleHeroesUsed = true;
                }
            });

            InitStartTime();
            SortPlayersByPlacement();
        }

        public void InitStartTime()
        {
            Player? minJoinTimePlayer = Players.MinBy(p => p.JoinTime);
            StartTime = minJoinTimePlayer != null ? minJoinTimePlayer.JoinTime : DateTimeOffset.UnixEpoch;
        }

        public void SortPlayersByPlacement()
        {
            Players.Sort((a, b) => a.Placement.CompareTo(b.Placement));
            if (Teams.Count > 0)
            {
                Teams.Sort((a, b) =>
                {
                    Player? playerA = Players.Find(p => p.Name == a[0]);
                    Player? playerB = Players.Find(p => p.Name == b[0]);
                    if (playerA == null || playerB == null) return 0; // Should never happen
                    return playerA.Placement.CompareTo(playerB.Placement);
                });
            }
        }

        public string ToCsvString()
        {
            List<string> player_name = new List<string>();
            List<int> player_deaths = new List<int>();
            List<string> player_hero = new List<string>();
            SortPlayersByPlacement();
            Players.ForEach(player =>
            {
                player_name.Add(player.Name);
                player_deaths.Add(player.Deaths);
                player_hero.Add(player.Hero);
            });

            string[] properties = new string[Constants.GameHeaders.Count];
            properties[Constants.GameHeaders.IndexOf("time")] = StartTime.ToString("s", System.Globalization.CultureInfo.InvariantCulture);
            properties[Constants.GameHeaders.IndexOf("map")] = Map;
            properties[Constants.GameHeaders.IndexOf("version")] = this.GameVersion.ToString();
            properties[Constants.GameHeaders.IndexOf("players")] = $"\"{String.Join(";", player_name)}\"";
            properties[Constants.GameHeaders.IndexOf("heroes")] = $"\"{String.Join(";", player_hero)}\"";
            properties[Constants.GameHeaders.IndexOf("deaths")] = $"\"{String.Join(";", player_deaths)}\"";
            properties[Constants.GameHeaders.IndexOf("playerCount")] = Players.Count.ToString();
            properties[Constants.GameHeaders.IndexOf("winner")] = player_name[0];
            if (IsTeam)
            {
                string winningTeam = String.Join(';', Teams[0]);
                properties[Constants.GameHeaders.IndexOf("winner")] = $"\"{winningTeam}\"";
            }
            properties[Constants.GameHeaders.IndexOf("gameLength")] = GameLength.ToString();
            properties[Constants.GameHeaders.IndexOf("isTeam")] = IsTeam.ToString();
            properties[Constants.GameHeaders.IndexOf("teams")] = $"\"{String.Join(';', Teams.Select(team => $"[{String.Join(";", team)}]"))}\"";
            return String.Join(",", properties);
        }

        public string PlayerCsvString(string name)
        {
            Player? player = Players.Find(p => p.Name == name);
            if (player == null)
            {
                throw new Exception($"No player found with name '{name}'.");
            }

            SortPlayersByPlacement();
            List<string> player_name = new List<string>();
            Players.ForEach(player => player_name.Add(player.Name));

            string[] properties = new string[Constants.PlayerHeaders.Count];
            properties[Constants.PlayerHeaders.IndexOf("time")] = StartTime.ToString("s", System.Globalization.CultureInfo.InvariantCulture);
            properties[Constants.PlayerHeaders.IndexOf("name")] = player.Name;
            properties[Constants.PlayerHeaders.IndexOf("placement")] = player.Placement.ToString();
            properties[Constants.PlayerHeaders.IndexOf("deaths")] = player.Deaths.ToString();
            properties[Constants.PlayerHeaders.IndexOf("hero")] = player.Hero;
            properties[Constants.PlayerHeaders.IndexOf("map")] = Map;
            properties[Constants.PlayerHeaders.IndexOf("playerCount")] = Players.Count.ToString();
            properties[Constants.PlayerHeaders.IndexOf("gameLength")] = GameLength.ToString();
            properties[Constants.PlayerHeaders.IndexOf("players")] = $"\"{String.Join(";", player_name)}\"";
            properties[Constants.PlayerHeaders.IndexOf("version")] = GameVersion.ToString();
            properties[Constants.PlayerHeaders.IndexOf("isTeam")] = IsTeam.ToString();
            properties[Constants.PlayerHeaders.IndexOf("teammates")] = $"\"{String.Join(";", player.TeamMates)}\"";
            return String.Join(",", properties);
        }

        public override string ToString()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public void WriteToJson(string filename)
        {
            string json = this.ToString();
            File.WriteAllText(filename, json);
        }

        // Returns a key uniquely identifying the game
        public string getHash()
        {
            return StartTime.ToString("s", System.Globalization.CultureInfo.InvariantCulture);
        }

        // Returns a key uniquely identifying the player profile for a game
        public string getHash(string playerName)
        {
            return playerName + "." + StartTime.ToString("s", System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
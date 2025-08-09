using System.Globalization;
using System.Runtime;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic.FileIO;

namespace BrawlhallaReplayReader
{
	class Program
	{
		static string sourceFolder = "replays";
		static string outputFolder = "output";
		static readonly string[] familiarPlayers = {};
		public static void Main(string[] args)
		{
			if (args.Length > 0 && args[0] == "a")
			{
				ProcessAllReplays();
				return;
			}
			Console.WriteLine("===== Brawlhalla Replay Reader =====");
			Console.WriteLine("\tTo process all replays, run with 'a' argument.");
			Console.WriteLine("\tExample: dotnet run a");
			Console.WriteLine("\tMake sure to modify the 'familiarPlayers' list in Program.cs to include the players you want to track.");
		}

		private static void ProcessAllReplays()
		{
			Dictionary<string, List<PlayerProfile>> playerResults = new Dictionary<string, List<PlayerProfile>>();
			List<GameInfo> allGames = new List<GameInfo>();
			HashSet<string> seen = new HashSet<string>();
			Dictionary<string, int> skippedCount = new Dictionary<string, int>();
			int skippedGames = 0;

			Console.WriteLine("Looking for existing records...");
			familiarPlayers.ToList().ForEach((player) =>
			{
				playerResults.Add(player, new List<PlayerProfile>());
				skippedCount.Add(player, 0);

				int count = 0;
				if (File.Exists($"{outputFolder}/{player}.csv"))
				{
					using (TextFieldParser csvParser = new TextFieldParser($"{outputFolder}/{player}.csv"))
					{
						csvParser.SetDelimiters(new string[] { "," });
						csvParser.HasFieldsEnclosedInQuotes = true;

						// Skip the row with the column names
						csvParser.ReadLine();
						while (!csvParser.EndOfData)
						{
							string[] fields = csvParser.ReadFields();
							try
							{
								DateTimeOffset startTime;
								DateTimeOffset.TryParseExact(fields[Constants.PlayerHeaders.IndexOf("time")], "s", System.Globalization.CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out startTime);
								PlayerProfile profile = new PlayerProfile
								{
									Name = fields[Constants.PlayerHeaders.IndexOf("name")],
									Placement = int.Parse(fields[Constants.PlayerHeaders.IndexOf("placement")]),
									Deaths = int.Parse(fields[Constants.PlayerHeaders.IndexOf("deaths")]),
									Hero = fields[Constants.PlayerHeaders.IndexOf("hero")],
									TeamMates = fields[Constants.PlayerHeaders.IndexOf("teammates")].Split(';').ToList(),
									GameInfo = new GameInfo
									{
										StartTime = startTime,
										Map = fields[Constants.PlayerHeaders.IndexOf("map")],
										Players = fields[Constants.PlayerHeaders.IndexOf("players")].Split(';').ToList(),
										GameLength = int.Parse(fields[Constants.PlayerHeaders.IndexOf("gameLength")]),
										Version = float.Parse(fields[Constants.PlayerHeaders.IndexOf("version")]),
										Checksum = uint.Parse(fields[Constants.PlayerHeaders.IndexOf("checksum")]),
										IsTeam = bool.Parse(fields[Constants.PlayerHeaders.IndexOf("isTeam")])
									}
								};
								playerResults[player].Add(profile);
								count += 1;
								seen.Add(player + "." + profile.GameInfo.StartTime.ToString("s", System.Globalization.CultureInfo.InvariantCulture));
							}
							catch (Exception e)
							{
								Console.WriteLine($"Error parsing CSV for {player}: {e.Message}");
							}
						}
						Console.WriteLine($"Loaded {count} existing results for {player} from CSV.");
					}
				}
				else
				{
					Console.WriteLine($"No existing records found for {player}.");
				}
			});

			// Load existing games from all_games.csv
			if (File.Exists($"{outputFolder}/all_games.csv"))
			{
				int count = 0;
				using (TextFieldParser csvParser = new TextFieldParser($"{outputFolder}/all_games.csv"))
				{
					csvParser.SetDelimiters(new string[] { "," });
					csvParser.HasFieldsEnclosedInQuotes = true;

					// Skip the row with the column names
					csvParser.ReadLine();
					while (!csvParser.EndOfData)
					{
						string[] fields = csvParser.ReadFields();
						try
						{
							DateTimeOffset startTime;
							DateTimeOffset.TryParseExact(fields[Constants.GameHeaders.IndexOf("time")], "s", System.Globalization.CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out startTime);

							GameInfo game = new GameInfo
							{
								StartTime = startTime,
								Map = fields[Constants.GameHeaders.IndexOf("map")],
								Players = fields[Constants.GameHeaders.IndexOf("players")].Split(';').ToList(),
								Heroes = fields[Constants.GameHeaders.IndexOf("heroes")].Split(';').ToList(),
								Deaths = fields[Constants.GameHeaders.IndexOf("deaths")].Split(';').ToList().Select(int.Parse).ToList(),
								GameLength = int.Parse(fields[Constants.GameHeaders.IndexOf("gameLength")]),
								Version = float.Parse(fields[Constants.GameHeaders.IndexOf("version")]),
								Checksum = uint.Parse(fields[Constants.GameHeaders.IndexOf("checksum")]),
								IsTeam = bool.Parse(fields[Constants.GameHeaders.IndexOf("isTeam")])
							};

							if (game.IsTeam)
							{
								string teamsString = fields[Constants.GameHeaders.IndexOf("teams")];
								string pattern = @"\[(.*?)\]";
								MatchCollection matches = Regex.Matches(teamsString, pattern, RegexOptions.IgnoreCase);
								
								foreach (var team in matches)
								{
									game.Teams.Add(team.ToString().Trim('[', ']').Split(';').ToList());
								}
							}
							List<List<string>> teams = new List<List<string>>();

							allGames.Add(game);
							count += 1;
							seen.Add(game.StartTime.ToString("s", System.Globalization.CultureInfo.InvariantCulture));
						}
						catch (Exception e)
						{
							Console.WriteLine($"Error parsing all_games.csv: {e.Message}");
						}
					}
				}
				Console.WriteLine($"Loaded {count} existing games from CSV.");
			}
			else
			{
				Console.WriteLine($"No existing games found in all_games.csv");
			}

			Console.WriteLine("Processing all replays...");
			System.IO.Directory.CreateDirectory(sourceFolder);
			string[] files = Directory.GetFiles("replays", "*.replay");
			Console.WriteLine($"Found {files.Length} replay files.");
			foreach (string file in files)
			{
				float version = float.Parse(file.Split(['[', ']'])[1]);
				if (version < 9.0f) continue;

				try
				{
					Replay replay = new(File.Open(file, FileMode.Open, FileAccess.Read), false);

					GameInfo gameInfo = new GameInfo
					{
						StartTime = DateTimeOffset.FromUnixTimeSeconds(replay.Entities[0].Player.ConnectionTime),
						Map = file.Split([']', '(', '.'])[2].Trim([' ', '\'']),
						Version = version,
						Checksum = replay.Checksum,
						GameLength = (int)replay.Length,
						IsTeam = replay.GameSettings.Flags.ToString().Contains("Teams") && replay.Entities.Count > 2
					};

					List<PlayerProfile> players = new List<PlayerProfile>();
					Dictionary<int, List<string>> teams = new Dictionary<int, List<string>>();
					int familiarPlayerCount = 0;

					foreach (var entity in replay.Entities)
					{
						if (familiarPlayers.Any((player) => player.ToLowerInvariant() == entity.Name.ToLowerInvariant()))
						{
							familiarPlayerCount++;
						}

						int deaths = 0;
						replay.Deaths.ToList().ForEach((death) =>
						{
							if (death.EntityID == entity.EntityID)
							{
								deaths += 1;
							}
						});

						if (gameInfo.IsTeam)
						{
							if (!teams.ContainsKey(entity.Player.Team))
							{
								teams[entity.Player.Team] = new List<string>();
							}
							teams[entity.Player.Team].Add(entity.Name);
						}

						PlayerProfile profile = new PlayerProfile
						{
							Name = entity.Name,
							Placement = replay.Results[entity.EntityID],
							Deaths = deaths,
							Hero = Constants.Heroes.ContainsKey((int)entity.Player.Heroes[0].HeroID) ? Constants.Heroes[(int)entity.Player.Heroes[0].HeroID] : entity.Player.Heroes[0].HeroID.ToString(),
							Team = entity.Player.Team,
							GameInfo = gameInfo
						};

						players.Add(profile);
					}

					if (familiarPlayerCount < 2) continue;

					players.Sort((a, b) => a.Placement.CompareTo(b.Placement));

					for (int i = 0; i < players.Count; i++)
					{
						gameInfo.Players.Add(players[i].Name);
						gameInfo.Heroes.Add(players[i].Hero);
						gameInfo.Deaths.Add(players[i].Deaths);

						if (gameInfo.IsTeam)
						{
							players[i].TeamMates = teams[players[i].Team];
						}

						// Hash this game result and skip if already seen
						if (!playerResults.ContainsKey(players[i].Name))
						{
							Console.WriteLine($"Unfamiliar player: {players[i].Name}. File: {file}");
						}
						else
						{
							string key = players[i].Name + "." + gameInfo.StartTime.ToString("s", System.Globalization.CultureInfo.InvariantCulture);
							if (seen.Contains(key))
							{
								skippedCount[players[i].Name] += 1;
								continue;
							}
							seen.Add(key);
							playerResults[players[i].Name].Add(players[i]);
						}
					}

					if (gameInfo.IsTeam)
					{
						gameInfo.Teams = teams.Values.ToList();
					}

					string gameKey = gameInfo.StartTime.ToString("s", System.Globalization.CultureInfo.InvariantCulture);
					if (seen.Contains(gameKey))
					{
						skippedGames += 1;
					}
					else
					{
						allGames.Add(gameInfo);
						seen.Add(gameKey);
					}
				}
				catch (Exception e)
				{
					Console.WriteLine($"Error processing {file}: {e.Message}");
				}
			}

			// Write output for each player
			playerResults.Keys.ToList().ForEach((player) =>
			{
				if (skippedCount[player] > 0)
				{
					Console.WriteLine($"Skipped {skippedCount[player]} duplicate entries for {player}.");
				}
				if (playerResults[player].Count == 0)
				{
					Console.WriteLine($"No results for {player}");
					return;
				}
				string outputFile = Path.Combine(outputFolder, $"{player}.csv");
				List<string> lines = new List<string>([string.Join(",", Constants.PlayerHeaders)]);

				playerResults[player].Sort((a, b) => a.GameInfo.StartTime.CompareTo(b.GameInfo.StartTime));
				playerResults[player].ForEach((profile) => lines.Add(profile.ToCsvString()));

				File.WriteAllLines(outputFile, lines);
				Console.WriteLine($"Processed {playerResults[player].Count} {player} results to {outputFile}");
			});

			// Write output for all games
			System.IO.Directory.CreateDirectory(outputFolder);
			string outputFile = Path.Combine(outputFolder, "all_games.csv");
			List<string> lines = new List<string>([string.Join(",", Constants.GameHeaders)]);

			allGames.Sort((a, b) => a.StartTime.CompareTo(b.StartTime));
			allGames.ForEach((game) => lines.Add(game.ToCsvString()));

			File.WriteAllLines(outputFile, lines);
			if (skippedGames > 0)
			{
				Console.WriteLine($"Skipped {skippedGames} duplicate games");
			}
			Console.WriteLine($"Processed {allGames.Count} games to {outputFile}");
			Console.WriteLine("All replays processed.");
		}
	}
}
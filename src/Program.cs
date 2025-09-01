using System.Globalization;
using System.Runtime;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic.FileIO;
using Microsoft.JavaScript.NodeApi;
using System.Reflection;
using Microsoft.JavaScript.NodeApi.Runtime;
using System.Threading.Tasks;
using System.IO.Enumeration; 
using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;
using BrawlhallaReplayReader.v8_06;

namespace BrawlhallaReplayReader
{
	class Program
	{
		static string sourceFolder = "replays";
		static string outputFolder = "output";
		static readonly string[] familiarPlayers = { };

		public static NodeEmbeddingThreadRuntime? NodeJsRuntime;

        [RequiresAssemblyFiles("Calls BrawlhallaReplayReader.Program.InitializeNodeJsPlatform()")]
        public static void Main(string[] args)
		{
			if (args.Length > 0 && args[0] == "a")
			{
				InitializeNodeJsPlatform();
				ProcessAllReplays();
				NodeJsRuntime?.Dispose();
				return;
			}
			if (args.Length == 0 || (args.Length > 0 && args[0].ToCharArray()[0] == 'd'))
			{
				InitializeNodeJsPlatform();
				GameReplay replay = ProcessGame("replays\\Old\\[7.03] SpiritRealm.replay");
				Console.WriteLine(replay.ToString());
				NodeJsRuntime?.Dispose();
				return;
			}
			Console.WriteLine("===== Brawlhalla Replay Reader =====");
			Console.WriteLine("\tTo process all replays, run with 'a' argument.");
			Console.WriteLine("\tExample: dotnet run a");
			Console.WriteLine("\tMake sure to modify the 'familiarPlayers' list in Program.cs to include the players you want to track.");
		}

        [RequiresAssemblyFiles("Calls System.Reflection.Assembly.Location")]
        private static void InitializeNodeJsPlatform()
		{
			string baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
			string libnodePath = Path.Combine(baseDir, "libnode.dll");
			NodeEmbeddingPlatform nodejsPlatform = new(new NodeEmbeddingPlatformSettings { LibNodePath = libnodePath });
			NodeJsRuntime = nodejsPlatform.CreateThreadRuntime(baseDir,
				new NodeEmbeddingRuntimeSettings
				{
					MainScript =
						"globalThis.require = require('module').createRequire(process.execPath);\n"
				}
			);
		}

		// Processes any version of replay file
		private static GameReplay ProcessGame(string filename, float version = 0.0f)
		{
			if (version == 0.0f)
			{
				try
				{
					version = Helpers.GetReplayFileVersion(filename);
				}
				catch (Exception e)
				{
					throw new Exception($"Could not parse the game version for file {filename}: {e.Message}");
				}
			}

			if (version <= 9.10f && version >= 9.08f)
			{
				v9_08.Replay replay = new(File.Open(filename, FileMode.Open, FileAccess.Read), false);
				return new GameReplay(replay, version);
			}
			else if (version >= 9.01f)
			{
				v9_01.Replay replay = new(File.Open(filename, FileMode.Open, FileAccess.Read), false);
				return new GameReplay(replay, version);
			}
			else if (version >= 8.07f)
			{
				v8_07.Replay replay = new(File.Open(filename, FileMode.Open, FileAccess.Read), false);
				return new GameReplay(replay, version);
			}
			else if (version >= 8.06f)
			{
				v8_06.Replay replay = new(File.Open(filename, FileMode.Open, FileAccess.Read), false);
				return new GameReplay(replay, version);
			}
			else if (version >= 8.00f)
			{
				v8_00.ReplayData? game = NodeJsRuntime?.Run(() =>
				{
					JSValue replayReader = NodeJsRuntime.Import("brawlhalla-replay-reader", "ReplayData");
					JSValue readFile = NodeJsRuntime.Import("fs", "readFileSync");
					JSValue file = readFile.CallAsConstructor(filename);
					JSValue stringify = NodeJsRuntime.Import("JSON");
					JSValue result = stringify.CallMethod("stringify", replayReader.CallMethod("ReadReplay", file));
					return JsonConvert.DeserializeObject<v8_00.ReplayData>(result.GetValueStringUtf16());
				});

				if (game == null)
				{
					throw new Exception($"ReplayData was null after processing via Node runtime");
				}
				return new GameReplay(game, version);
			}
			else if (version > 6.04f && version < 7.03)
			{
				v8_00.ReplayData? game = NodeJsRuntime?.Run(() =>
				{
					JSValue replayReader = NodeJsRuntime.Import("brawlhalla-replay-reader-1-0-1", "ReplayData");
					JSValue readFile = NodeJsRuntime.Import("fs", "readFileSync");
					JSValue file = readFile.CallAsConstructor(filename);
					JSValue stringify = NodeJsRuntime.Import("JSON");
					JSValue result = stringify.CallMethod("stringify", replayReader.CallMethod("ReadReplay", file));
					return JsonConvert.DeserializeObject<v8_00.ReplayData>(result.GetValueStringUtf16());
				});

				if (game == null)
				{
					throw new Exception($"ReplayData was null after processing via Node runtime");
				}
				return new GameReplay(game, version);
			}

			throw new Exception($"Unsupported game version {version}. Supported versions are 6.05-7.02, 8.00-9.10");
		}

		private static void ProcessAllReplays()
		{
			Dictionary<string, List<GameReplay>> playerResults = new Dictionary<string, List<GameReplay>>();
			List<GameReplay> allGames = new List<GameReplay>();
			HashSet<string> seen = new HashSet<string>();
			Dictionary<string, int> skippedCount = new Dictionary<string, int>();
			int skippedGames = 0;

			Console.WriteLine("Looking for existing records...");
			familiarPlayers.ToList().ForEach((player) =>
			{
				playerResults.Add(player, new List<GameReplay>());
				skippedCount.Add(player, 0);

				/*int count = 0;
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
				}*/
			});

			// Load existing games from all_games.csv
			/*if (File.Exists($"{outputFolder}/all_games.csv"))
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
			}*/

			Console.WriteLine("Processing all replays...");
			System.IO.Directory.CreateDirectory(sourceFolder);
			string[] files = Directory.GetFiles("replays", "*.replay", System.IO.SearchOption.AllDirectories);
			Console.WriteLine($"Found {files.Length} replay files.");
			Dictionary<float, int> unsupportedVersions = new Dictionary<float, int>();
			Dictionary<int, string> unknownMaps = new Dictionary<int, string>();
			foreach (string file in files)
			{
				float version = 0.0f;
				GameReplay replay;
				try
				{
					version = Helpers.GetReplayFileVersion(file);
					if (!Helpers.IsVersionValid(version))
					{
						if (!unsupportedVersions.ContainsKey(version))
						{
							unsupportedVersions[version] = 0;
						}
						unsupportedVersions[version] += 1;
						continue;
					}
					replay = ProcessGame(file);
					var isNumeric = int.TryParse(replay.Map, out int n);
					if (isNumeric && !unknownMaps.ContainsKey(n))
					{
						unknownMaps.Add(n, file.Split(' ')[1]);
					}

					if (version < 8.0f)
					{
						replay.WriteToJson(file + ".json");
					}

					int familiarPlayerCount = replay.Players.Count(player => familiarPlayers.Contains(player.Name));
					if (familiarPlayerCount < 2)
					{
						continue;
					}
					if (familiarPlayerCount < replay.Players.Count)
					{
						throw new Exception($"Unfamiliar players: {String.Join(", ", replay.Players.Where(player => !familiarPlayers.Contains(player.Name)).Select(player => player.Name))}");
					}

					replay.Players.ForEach(player =>
					{
						if (!playerResults.ContainsKey(player.Name))
						{
							Console.WriteLine($"Unfamiliar player: {player.Name}. File: {file}");
						}
						else
						{
							string key = replay.getHash(player.Name);
							if (seen.Contains(key))
							{
								skippedCount[player.Name] += 1;
								return;
							}
							seen.Add(key);
							playerResults[player.Name].Add(replay);
						}
					});

					string gameKey = replay.getHash();
					if (seen.Contains(gameKey))
					{
						skippedGames += 1;
						continue;
					}
					else
					{
						allGames.Add(replay);
						seen.Add(gameKey);
					}
				}
				catch (Exception e)
				{
					Console.WriteLine($"Error reading replay ${file}: {e.Message}");
					continue;
				}
			}

			if (unknownMaps.Keys.Count > 0)
			{
				Console.WriteLine("\nUnknown maps");
				int[] keys = unknownMaps.Keys.ToArray();
				Array.Sort(keys);
				keys.ToList().ForEach(key => Console.WriteLine(key + " - " + unknownMaps[key]));
			}

			if (unsupportedVersions.Count > 0)
				{
					Console.WriteLine("\nUnsupported versions found:");
					foreach (var version in unsupportedVersions)
					{
						Console.WriteLine($"Version {version.Key} - {version.Value} files");
					}
				}

			List<string> combinedLines = new List<string>([string.Join(",", Constants.PlayerHeaders)]);

			// Write output for each player
			Console.WriteLine();
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

				playerResults[player].Sort((a, b) => a.StartTime.CompareTo(b.StartTime));
				playerResults[player].ForEach(replay =>
				{
					lines.Add(replay.PlayerCsvString(player));
					combinedLines.Add(replay.PlayerCsvString(player));
				});

				File.WriteAllLines(outputFile, lines);
				Console.WriteLine($"Processed {playerResults[player].Count} {player} results to {outputFile}");
			});

			File.WriteAllLines(Path.Combine(outputFolder, "combined_results.csv"), combinedLines);

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
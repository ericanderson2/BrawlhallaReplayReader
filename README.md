This is a tool to read Brawlhalla replay files. The goal of this project is to create a "version agnostic" replay parser, that will be able to parse replays across many different game versions without the user having to track down the correct tool for the version.

## Supported game versions
6.05 - 7.02, 8.00 - 9.10

## Usage

This is a work in progress project, and there isn't an official release. But, if you're interested in trying it out, you'll find `Program.cs` and the `ProcessGame` function helpful.

Process a replay file:
```
GameReplay replay = ProcessGame("replays/[7.02] SpiritRealm.replay");
Console.WriteLine(replay);
```

Save replay to JSON:
```
replay.WriteToJson("game.json");
```

If you are processing a file older than v8.00, you'll need to initialize and dispose of the Node runtime:
```
InitializeNodeJsPlatform();
// process all of your files
NodeJsRuntime?.Dispose();
```

## "League Data" collection
Another function of this project is to collect statistics from games that have happened between a specified group of players (a "league").

1. Create a folder in the project called "replays" and copy your Brawlhalla replay files there. You must use the original name for all files, which includes the game version in brackets ([9.10], etc.)

2. Edit the `familiarPlayers` array in `Program.cs` and add all the players you are interested in tracking.

3. Build the project, and run it with:
```sh
dotnet run a
```

A folder called "output" will be generated, with a CSV file for each individual player, and an `all_games` file containing all the records.

### Notes
* The player array is sorted by placement in both the all_games and individual player files
* The program will read and preserve old data if you leave the .CSV files in the output folder. So, if you have a friend's replay files with duplicate names to your own, you can process the replays in 2 separate batches, and they will be aggregated in the same output files.

## Credits
The replay reading code comes from the [Node package by itselectroz](https://github.com/itselectroz/brawlhalla-replay-reader) and the [.NET project by Talafhah1](https://github.com/Talafhah1/BrawlhallaReplayReader)



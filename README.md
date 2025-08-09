This is a tool to collect Brawlhalla stats in a CSV format for easy analysis. The purpose of this project is to analyze games played between a known group of players (a "league" of sorts). Stats will only be collected for games that include 2 or more of the selected players.

## Usage
1. Create a folder in the project called "replays" and copy your Brawlhalla replay files there. You must use the original name for all files, which includes the game version in brackets ([9.10], etc.)

2. Edit the `familiarPlayers` array in `Program.cs` and add all the players you are interested in tracking.

3. Build the project, and run it with:
```sh
dotnet run a
```

A folder called "output" will be generated, with a CSV file for each individual player, and an `all_games` file containing all the records.

## Notes
* The player array is sorted by placement in both the all_games and individual player files
* The program will read and preserve old data if you leave the .CSV files in the output folder. So, if you have a friend's replay files with duplicate names to your own, you can process the replays in 2 separate batches, and they will be aggregated in the same output files.

## Supported game versions
9.01 - 9.10

// ReplayData.cs
// Contains classes for ReplayData and its sub-objects

using System.Collections.Generic;

namespace BrawlhallaReplayReader.v8_00
{
    public class ReplayData
    {
        public int Length { get; set; }
        public Dictionary<int, int> Results { get; set; }
        public List<Death> Deaths { get; set; }
        public Dictionary<int, List<Input>> Inputs { get; set; }
        public int RandomSeed { get; set; }
        public int Version { get; set; }
        public int PlaylistId { get; set; }
        public string? PlaylistName { get; set; }
        public bool OnlineGame { get; set; }
        public GameSettings? GameSettings { get; set; }
        public int LevelId { get; set; }
        public int HeroCount { get; set; }
        public List<Entity> Entities { get; set; }
    }

    public class Death
    {
        public int EntityId { get; set; }
        public int Timestamp { get; set; }
    }

    public class Input
    {
        public int Timestamp { get; set; }
        public int InputState { get; set; }
    }

    public class GameSettings
    {
        public int Flags { get; set; }
        public int MaxPlayers { get; set; }
        public int Duration { get; set; }
        public int RoundDuration { get; set; }
        public int StartingLives { get; set; }
        public int ScoringType { get; set; }
        public int ScoreToWin { get; set; }
        public int GameSpeed { get; set; }
        public int DamageRatio { get; set; }
        public int LevelSetID { get; set; }
    }

    public class Entity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public PlayerData Data { get; set; }
    }

    public class PlayerData
    {
        public int ColourId { get; set; }
        public int SpawnBotId { get; set; }
        public int EmitterId { get; set; }
        public int PlayerThemeId { get; set; }
        public List<int> Taunts { get; set; }
        public int WinTaunt { get; set; }
        public int LoseTaunt { get; set; }
        public List<int> Unknown1 { get; set; }
        public int AvatarId { get; set; }
        public int Team { get; set; }
        public int Unknown2 { get; set; }
        public List<HeroData> Heroes { get; set; }
        public bool Bot { get; set; }
    }

    public class HeroData
    {
        public int HeroId { get; set; }
        public int CostumeId { get; set; }
        public int Stance { get; set; }
        public int WeaponSkins { get; set; }
    }
}

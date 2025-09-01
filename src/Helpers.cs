namespace BrawlhallaReplayReader
{
    public static class Helpers
    {
        public static string GetHeroName(int heroId)
        {
            return Constants.Heroes.ContainsKey(heroId) ? Constants.Heroes[heroId] : heroId.ToString();
        }

        public static string GetHeroName(string heroId)
        {
            var isNumeric = int.TryParse(heroId, out int n);
            if (!isNumeric) return heroId;
            return GetHeroName(n);
        }

        public static string GetLevelName(int levelId)
        {
            return Constants.Levels.ContainsKey(levelId) ? Constants.Levels[levelId] : levelId.ToString();
        }

        public static string GetLevelName(string levelId)
        {
            var isNumeric = int.TryParse(levelId, out int n);
            if (!isNumeric) return levelId;
            return GetLevelName(n);
        }

        public static bool IsVersionValid(float version)
        {
            return (version >= 6.05 && version <= 7.02) || (version >= 8.00 && version <= 9.11f);
        }

        public static float GetReplayFileVersion(string filename)
        {
            return float.Parse(filename.Split(['[', ']'])[1]);
        }
    }
}


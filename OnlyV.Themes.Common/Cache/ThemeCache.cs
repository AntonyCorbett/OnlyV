namespace OnlyV.Themes.Common.Cache
{
    using System.Collections.Concurrent;
    using System.Windows.Media;

    public class ThemeCache
    {
        private readonly ConcurrentDictionary<string, ThemeCacheEntry> _cache = new ConcurrentDictionary<string, ThemeCacheEntry>();

        public void Purge()
        {
            _cache.Clear();
        }

        public void Add(string themePath, ThemeCacheEntry entry)
        {
            _cache.TryAdd(themePath, entry);
        }

        public ThemeCacheEntry Get(string themePath)
        {
            if (string.IsNullOrEmpty(themePath))
            {
                return null;
            }

            _cache.TryGetValue(themePath, out var result);
            return result;
        }
    }
}

using Microsoft.Extensions.Configuration;

namespace BobsBookstoreClassic.Data
{
    /// <summary>
    /// Static configuration accessor that wraps IConfiguration for backward compatibility.
    /// Must be initialized at startup via Initialize(IConfiguration).
    /// </summary>
    public sealed class BookstoreConfiguration
    {
        private static readonly Dictionary<string, string> _appSettings = new(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, string> _connectionStrings = new(StringComparer.OrdinalIgnoreCase);

        private BookstoreConfiguration() { }

        /// <summary>Call once at application startup from Program.cs.</summary>
        public static void Initialize(IConfiguration configuration)
        {
            foreach (var item in configuration.AsEnumerable())
            {
                if (item.Value != null)
                    _appSettings[item.Key] = item.Value;
            }

            var connStrSection = configuration.GetSection("ConnectionStrings");
            foreach (var item in connStrSection.GetChildren())
            {
                if (item.Value != null)
                    _connectionStrings[item.Key] = item.Value;
            }
        }

        public static void AddSetting(string key, string value)
        {
            _appSettings[key] = value;
        }

        public static string GetSetting(string key)
        {
            return _appSettings.TryGetValue(key, out var val) ? val : string.Empty;
        }

        public static T GetSetting<T>(string key)
        {
            var value = GetSetting(key);
            return (T)Convert.ChangeType(value, typeof(T));
        }

        public static void AddConnectionString(string key, string value)
        {
            _connectionStrings[key] = value;
        }

        public static string GetConnectionString(string key)
        {
            return _connectionStrings.TryGetValue(key, out var val) ? val : string.Empty;
        }
    }
}

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace BobsBookstoreClassic.Data
{
    public sealed class BookstoreConfiguration
    {
        private static readonly Lazy<BookstoreConfiguration> Lazy = new Lazy<BookstoreConfiguration>(() => new BookstoreConfiguration());

        private static BookstoreConfiguration Instance => Lazy.Value;

        private readonly Dictionary<string, string> _appSettings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> _connectionStrings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private BookstoreConfiguration() { }

        /// <summary>
        /// Called from Program.cs during startup to seed settings from IConfiguration.
        /// </summary>
        public static void Initialize(IConfiguration configuration)
        {
            foreach (var kvp in configuration.AsEnumerable())
            {
                if (kvp.Value != null)
                {
                    Instance._appSettings[kvp.Key] = kvp.Value;
                }
            }

            foreach (var cs in configuration.GetSection("ConnectionStrings").GetChildren())
            {
                Instance._connectionStrings[cs.Key] = cs.Value ?? string.Empty;
            }

            // Also store env-var overrides (key format: KEY becomes KEY)
            foreach (var key in Instance._appSettings.Keys)
            {
                var envVal = Environment.GetEnvironmentVariable(key);
                if (envVal != null)
                    Instance._appSettings[key] = envVal;
            }
        }

        public static void AddSetting(string key, string value)
        {
            Instance._appSettings[key] = value;
        }

        public static string GetSetting(string key)
        {
            Instance._appSettings.TryGetValue(key, out var value);
            return value ?? string.Empty;
        }

        public static T GetSetting<T>(string key)
        {
            var value = GetSetting(key);
            return (T)Convert.ChangeType(value, typeof(T));
        }

        public static void AddConnectionString(string key, string value)
        {
            Instance._connectionStrings[key] = value;
        }

        public static string GetConnectionString(string key)
        {
            Instance._connectionStrings.TryGetValue(key, out var value);
            return value ?? string.Empty;
        }
    }
}

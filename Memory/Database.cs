using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Eliza.Memory
{
    // Модель данных для сохранения
    public class UserProfile
    {
        public int TotalInteractions { get; set; } = 0;
        public int ApologiesCount { get; set; } = 0;
        public List<string> HistoryLogs { get; set; } = new List<string>();
    }

    public static class Database
    {
        private static readonly string DbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "eliza_memory.json");
        private static UserProfile _profile = new UserProfile();

        public static UserProfile Current => _profile;

        public static void Load()
        {
            try
            {
                if (File.Exists(DbPath))
                {
                    string json = File.ReadAllText(DbPath);
                    _profile = JsonSerializer.Deserialize<UserProfile>(json) ?? new UserProfile();
                }
            }
            catch 
            {
                _profile = new UserProfile();
            }
        }

        public static void Save()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(_profile, options);
                File.WriteAllText(DbPath, json);
            }
            catch { }
        }

        public static void AddLog(string log)
        {
            _profile.HistoryLogs.Add($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {log}");
            if (_profile.HistoryLogs.Count > 50) 
            {
                _profile.HistoryLogs.RemoveAt(0); // Держим только последние 50 записей
            }
            Save();
        }
    }
}
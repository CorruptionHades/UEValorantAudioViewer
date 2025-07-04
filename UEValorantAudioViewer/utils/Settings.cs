using System.IO;
using System.Text.Json;

namespace UEValorantAudioViewer.utils;

public class Settings {
    
    public class AppSettings
    {
        public string PaksFolder { get; set; }
        public string AesKey { get; set; }
        public string OutputFolder { get; set; }
    }
    
    public static AppSettings settings = new();
    
    public static void SaveSettings() {
        string json = JsonSerializer.Serialize(settings);
        string settingsFilePath = Path.Combine(Directory.GetCurrentDirectory(), "settings.json");

        File.WriteAllText(settingsFilePath, json);
    }

    public static void LoadSettings() {
        Console.WriteLine("Loading settings from file...");
        string settingsFilePath = Path.Combine(Directory.GetCurrentDirectory(), "settings.json");
        if (File.Exists(settingsFilePath))
        {
            string json = File.ReadAllText(settingsFilePath);
            settings = JsonSerializer.Deserialize<AppSettings>(json);
        }
    }
}
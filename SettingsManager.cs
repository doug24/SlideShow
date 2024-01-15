using System.IO;
using System.Reflection;
using System.Text.Json;

namespace SlideShow;

public abstract class SettingsManager<T> where T : SettingsManager<T>, new()
{
    public static T Instance { get; private set; } = new();

    private static readonly JsonSerializerOptions serializerOptions = new() { WriteIndented = true };
    private static readonly string filePath = GetLocalFilePath();

    private static string GetLocalFilePath()
    {
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string appName = Assembly.GetEntryAssembly()?.GetName().Name ?? string.Empty;
        string fileName = $"{typeof(T).Name}.json";
        return Path.Combine(appData, appName, fileName);
    }

    public static void Load()
    {
        if (File.Exists(filePath))
        {
            var data = JsonSerializer.Deserialize<T>(File.ReadAllText(filePath));
            if (data != null)
            {
                Instance = data;
            }
        }
    }

    public static void Save()
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        string json = JsonSerializer.Serialize(SettingsManager<T>.Instance, serializerOptions);
        File.WriteAllText(filePath, json);
    }
}
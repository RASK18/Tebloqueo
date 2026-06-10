using System.Text.Json;

namespace Tebloqueo;

internal sealed class AppSettings
{
    public const int DefaultIntervalMinutes = 15;
    public const int MinimumIntervalMinutes = 1;
    public const int MaximumIntervalMinutes = 1440;

    public int IntervalMinutes { get; set; } = DefaultIntervalMinutes;
    public bool NotificationsEnabled { get; set; } = true;

    public void Normalize() =>
        IntervalMinutes = Math.Clamp(IntervalMinutes, MinimumIntervalMinutes, MaximumIntervalMinutes);
}

internal sealed class SettingsStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true };

    public string SettingsPath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Tebloqueo",
        "settings.json");

    public AppSettings Load()
    {
        try
        {
            if (!File.Exists(SettingsPath))
            {
                return new AppSettings();
            }

            var settings = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(SettingsPath)) ?? new AppSettings();
            settings.Normalize();
            return settings;
        }
        catch
        {
            return new AppSettings();
        }
    }

    public void Save(AppSettings settings)
    {
        settings.Normalize();
        var directory = Path.GetDirectoryName(SettingsPath)!;
        Directory.CreateDirectory(directory);

        var temporaryPath = SettingsPath + ".tmp";
        File.WriteAllText(temporaryPath, JsonSerializer.Serialize(settings, SerializerOptions));
        File.Move(temporaryPath, SettingsPath, true);
    }
}

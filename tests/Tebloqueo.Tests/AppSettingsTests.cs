namespace Tebloqueo.Tests;

public sealed class AppSettingsTests
{
    [Theory]
    [InlineData(-5, AppSettings.MinimumIntervalMinutes)]
    [InlineData(5, 5)]
    [InlineData(5000, AppSettings.MaximumIntervalMinutes)]
    public void NormalizeClampsInterval(int value, int expected)
    {
        var settings = new AppSettings { IntervalMinutes = value };

        settings.Normalize();

        Assert.Equal(expected, settings.IntervalMinutes);
    }

    [Fact]
    public void NormalizeResetsUnknownIsp()
    {
        var settings = new AppSettings { SelectedIsp = (Isp)999 };

        settings.Normalize();

        Assert.Equal(Isp.Todos, settings.SelectedIsp);
    }

    [Fact]
    public void SaveAndLoadPreservesAllSettings()
    {
        var directory = Path.Combine(Path.GetTempPath(), "Tebloqueo.Tests", Guid.NewGuid().ToString());
        var store = new SettingsStore(Path.Combine(directory, "settings.json"));

        try
        {
            store.Save(new AppSettings
            {
                IntervalMinutes = 10,
                NotificationsEnabled = false,
                SelectedIsp = Isp.Orange
            });

            var settings = store.Load();

            Assert.Equal(10, settings.IntervalMinutes);
            Assert.False(settings.NotificationsEnabled);
            Assert.Equal(Isp.Orange, settings.SelectedIsp);
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }
}

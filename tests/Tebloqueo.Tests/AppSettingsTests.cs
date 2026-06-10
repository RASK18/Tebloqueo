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
}

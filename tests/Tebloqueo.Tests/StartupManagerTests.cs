namespace Tebloqueo.Tests;

public sealed class StartupManagerTests
{
    [Fact]
    public void NeedsPathRepairReturnsFalseForCurrentQuotedPath()
    {
        const string path = @"C:\Apps\Tebloqueo.exe";

        var needsRepair = StartupManager.NeedsPathRepair(@"""C:\Apps\Tebloqueo.exe""", path);

        Assert.False(needsRepair);
    }

    [Fact]
    public void NeedsPathRepairIgnoresPathCasing()
    {
        const string path = @"C:\Apps\Tebloqueo.exe";

        var needsRepair = StartupManager.NeedsPathRepair(@"""c:\apps\tebloqueo.exe""", path);

        Assert.False(needsRepair);
    }

    [Fact]
    public void NeedsPathRepairReturnsTrueAfterExecutableMoves()
    {
        const string currentPath = @"D:\Portable\Tebloqueo.exe";

        var needsRepair = StartupManager.NeedsPathRepair(@"""C:\Apps\Tebloqueo.exe""", currentPath);

        Assert.True(needsRepair);
    }

    [Fact]
    public void FormatCommandQuotesExecutablePath()
    {
        var command = StartupManager.FormatCommand(@"C:\My Apps\Tebloqueo.exe");

        Assert.Equal(@"""C:\My Apps\Tebloqueo.exe""", command);
    }
}

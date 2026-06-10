namespace Tebloqueo;

static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        if (UpdateApplier.TryApply(args))
        {
            return;
        }

        var launchOptions = UpdateApplier.ReadLaunchOptions(args);
        UpdateApplier.BeginCleanup(launchOptions.CleanupPath);

        using var mutex = new Mutex(true, @"Local\Tebloqueo.SingleInstance", out var isFirstInstance);
        if (!isFirstInstance)
        {
            return;
        }

        ApplicationConfiguration.Initialize();
        Application.Run(new TrayApplicationContext(launchOptions.UpdateError));
    }
}

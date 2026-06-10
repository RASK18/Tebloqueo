using System.Diagnostics;

namespace Tebloqueo;

internal sealed record LaunchOptions(string? CleanupPath, string? UpdateError);

internal static class UpdateApplier
{
    private const string ApplyArgument = "--apply-update";
    private const string CleanupArgument = "--cleanup-update";
    private const string ErrorArgument = "--update-error";

    public static bool TryApply(string[] args)
    {
        var applyIndex = Array.IndexOf(args, ApplyArgument);
        if (applyIndex < 0)
        {
            return false;
        }

        if (applyIndex + 2 >= args.Length ||
            !int.TryParse(args[applyIndex + 2], out var processId))
        {
            return true;
        }

        var targetPath = args[applyIndex + 1];
        var sourcePath = Environment.ProcessPath!;
        WaitForProcess(processId);

        string? error = null;
        if (!TryReplace(sourcePath, targetPath, out error))
        {
            StartTarget(targetPath, sourcePath, error);
            return true;
        }

        StartTarget(targetPath, sourcePath, null);
        return true;
    }

    public static LaunchOptions ReadLaunchOptions(string[] args) =>
        new(ReadValue(args, CleanupArgument), ReadValue(args, ErrorArgument));

    public static void BeginCleanup(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        _ = Task.Run(() =>
        {
            for (var attempt = 0; attempt < 30; attempt++)
            {
                try
                {
                    File.Delete(path);
                    return;
                }
                catch
                {
                    Thread.Sleep(250);
                }
            }
        });
    }

    public static void Launch(string downloadedPath)
    {
        var targetPath = Environment.ProcessPath
            ?? throw new InvalidOperationException("No se pudo determinar la ruta del ejecutable actual.");

        var startInfo = new ProcessStartInfo(downloadedPath) { UseShellExecute = false };
        startInfo.ArgumentList.Add(ApplyArgument);
        startInfo.ArgumentList.Add(targetPath);
        startInfo.ArgumentList.Add(Environment.ProcessId.ToString());

        _ = Process.Start(startInfo)
            ?? throw new InvalidOperationException("No se pudo iniciar el actualizador.");
    }

    private static bool TryReplace(string sourcePath, string targetPath, out string? error)
    {
        var stagedPath = targetPath + ".new";
        error = null;

        for (var attempt = 0; attempt < 20; attempt++)
        {
            try
            {
                File.Copy(sourcePath, stagedPath, true);
                File.Move(stagedPath, targetPath, true);
                return true;
            }
            catch (Exception exception)
            {
                error = exception.Message;
                Thread.Sleep(250);
            }
        }

        try
        {
            File.Delete(stagedPath);
        }
        catch
        {
            // Ignore cleanup errors; the original executable is still intact.
        }

        return false;
    }

    private static void WaitForProcess(int processId)
    {
        try
        {
            using var process = Process.GetProcessById(processId);
            process.WaitForExit(30_000);
        }
        catch
        {
            // The process already exited.
        }
    }

    private static void StartTarget(string targetPath, string cleanupPath, string? error)
    {
        try
        {
            var startInfo = new ProcessStartInfo(targetPath) { UseShellExecute = false };
            startInfo.ArgumentList.Add(CleanupArgument);
            startInfo.ArgumentList.Add(cleanupPath);

            if (!string.IsNullOrWhiteSpace(error))
            {
                startInfo.ArgumentList.Add(ErrorArgument);
                startInfo.ArgumentList.Add(error);
            }

            Process.Start(startInfo);
        }
        catch
        {
            // There is no remaining process available to report this failure.
        }
    }

    private static string? ReadValue(string[] args, string argument)
    {
        var index = Array.IndexOf(args, argument);
        return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
    }
}

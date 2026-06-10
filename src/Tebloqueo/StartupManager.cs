using Microsoft.Win32;

namespace Tebloqueo;

internal sealed class StartupManager
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "Tebloqueo";

    public bool IsEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, false);
        return key?.GetValue(ValueName) is string value && !string.IsNullOrWhiteSpace(value);
    }

    public bool RepairPathIfEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true);
        if (key?.GetValue(ValueName) is not string existingValue || string.IsNullOrWhiteSpace(existingValue))
        {
            return false;
        }

        var currentPath = GetCurrentPath();
        if (!NeedsPathRepair(existingValue, currentPath))
        {
            return false;
        }

        key.SetValue(ValueName, FormatCommand(currentPath));
        return true;
    }

    public void SetEnabled(bool enabled)
    {
        using var key = Registry.CurrentUser.CreateSubKey(RunKeyPath, true)
            ?? throw new InvalidOperationException("No se pudo abrir el registro de inicio de Windows.");

        if (enabled)
        {
            key.SetValue(ValueName, FormatCommand(GetCurrentPath()));
        }
        else
        {
            key.DeleteValue(ValueName, false);
        }
    }

    internal static bool NeedsPathRepair(string existingValue, string currentPath) =>
        !string.Equals(existingValue.Trim(), FormatCommand(currentPath), StringComparison.OrdinalIgnoreCase);

    internal static string FormatCommand(string executablePath) => $"\"{executablePath}\"";

    private static string GetCurrentPath() =>
        Environment.ProcessPath
        ?? throw new InvalidOperationException("No se pudo determinar la ruta actual de Tebloqueo.");
}

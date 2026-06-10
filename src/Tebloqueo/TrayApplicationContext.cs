using System.Reflection;

namespace Tebloqueo;

internal sealed class TrayApplicationContext : ApplicationContext
{
    private readonly SettingsStore _settingsStore = new();
    private readonly StartupManager _startupManager = new();
    private readonly AppSettings _settings;
    private readonly HttpClient _httpClient;
    private readonly StatusClient _statusClient;
    private readonly UpdateService _updateService;
    private readonly NotifyIcon _notifyIcon;
    private readonly System.Windows.Forms.Timer _timer;
    private readonly ToolStripMenuItem _statusItem;
    private readonly ToolStripMenuItem _lastCheckItem;
    private readonly ToolStripMenuItem _intervalItem;
    private readonly ToolStripMenuItem _notificationsItem;
    private readonly ToolStripMenuItem _startupItem;
    private readonly Icon _yesIcon = IconFactory.Create("SI", Color.FromArgb(196, 36, 36));
    private readonly Icon _noIcon = IconFactory.Create("NO", Color.FromArgb(30, 145, 76));
    private readonly Icon _unknownIcon = IconFactory.Create("?", Color.FromArgb(92, 99, 112));
    private readonly string? _launchUpdateError;
    private bool _isRefreshing;
    private bool _hasCompletedFirstCheck;
    private BlockingState _currentState = BlockingState.Unknown;

    public TrayApplicationContext(string? launchUpdateError)
    {
        _launchUpdateError = launchUpdateError;
        _settings = _settingsStore.Load();
        _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
        _statusClient = new StatusClient(_httpClient);
        _updateService = new UpdateService(_httpClient, GetCurrentVersion());

        _statusItem = new ToolStripMenuItem("Estado: ?") { Enabled = false };
        _lastCheckItem = new ToolStripMenuItem("Última comprobación: pendiente") { Enabled = false };
        var versionItem = new ToolStripMenuItem($"Versión: {GetCurrentVersion().ToString(3)}") { Enabled = false };
        var refreshItem = new ToolStripMenuItem("Comprobar estado ahora");
        _intervalItem = new ToolStripMenuItem();
        _notificationsItem = new ToolStripMenuItem("Mostrar notificaciones") { CheckOnClick = true };
        _startupItem = new ToolStripMenuItem("Iniciar con Windows") { CheckOnClick = true };
        var exitItem = new ToolStripMenuItem("Salir");

        refreshItem.Click += async (_, _) => await RefreshStatusAsync();
        _intervalItem.Click += (_, _) => ChangeInterval();
        _notificationsItem.Click += (_, _) => ToggleNotifications();
        _startupItem.Click += (_, _) => ToggleStartup();
        exitItem.Click += (_, _) => ExitThread();

        _notificationsItem.Checked = _settings.NotificationsEnabled;
        _startupItem.Checked = ReadStartupState();
        UpdateIntervalMenuText();

        var menu = new ContextMenuStrip();
        menu.Items.AddRange([
            _statusItem,
            _lastCheckItem,
            versionItem,
            new ToolStripSeparator(),
            refreshItem,
            _intervalItem,
            _notificationsItem,
            _startupItem,
            new ToolStripSeparator(),
            exitItem
        ]);

        _notifyIcon = new NotifyIcon
        {
            ContextMenuStrip = menu,
            Icon = _unknownIcon,
            Text = "Tebloqueo: comprobación pendiente",
            Visible = true
        };
        _notifyIcon.DoubleClick += async (_, _) => await RefreshStatusAsync();

        _timer = new System.Windows.Forms.Timer { Interval = IntervalMilliseconds(_settings.IntervalMinutes) };
        _timer.Tick += async (_, _) => await RefreshStatusAsync();

        Application.Idle += OnFirstIdle;
    }

    private async void OnFirstIdle(object? sender, EventArgs eventArgs)
    {
        Application.Idle -= OnFirstIdle;

        if (!string.IsNullOrWhiteSpace(_launchUpdateError))
        {
            ShowBalloon("Actualización no aplicada", Shorten(_launchUpdateError, 180), ToolTipIcon.Warning);
        }

        var updateResult = await _updateService.CheckAndDownloadAsync();
        if (!string.IsNullOrWhiteSpace(updateResult.Error))
        {
            ShowBalloon("No se pudo comprobar la actualización", Shorten(updateResult.Error, 180), ToolTipIcon.Warning);
        }
        else if (!string.IsNullOrWhiteSpace(updateResult.DownloadedPath))
        {
            try
            {
                UpdateApplier.Launch(updateResult.DownloadedPath);
                ExitThread();
                return;
            }
            catch (Exception exception)
            {
                ShowBalloon("No se pudo aplicar la actualización", Shorten(exception.Message, 180), ToolTipIcon.Warning);
            }
        }

        await RefreshStatusAsync();
        _timer.Start();
    }

    private async Task RefreshStatusAsync()
    {
        if (_isRefreshing)
        {
            return;
        }

        _isRefreshing = true;
        try
        {
            var status = await _statusClient.CheckAsync();
            ApplyStatus(status);
        }
        finally
        {
            _isRefreshing = false;
        }
    }

    private void ApplyStatus(BlockingStatus status)
    {
        var previousState = _currentState;
        _currentState = status.State;

        var stateText = StateText(status.State);
        _notifyIcon.Icon = StateIcon(status.State);
        _statusItem.Text = status.IpCount is int count
            ? $"Estado: {stateText} ({count} IP)"
            : $"Estado: {stateText}";
        _lastCheckItem.Text = $"Última comprobación: {DateTime.Now:G}";
        _notifyIcon.Text = Shorten(
            status.Error is null
                ? $"Tebloqueo: {stateText} - {status.IpCount} IP"
                : $"Tebloqueo: ? - {status.Error}",
            127);

        var shouldNotify = _settings.NotificationsEnabled &&
                           (!_hasCompletedFirstCheck
                               ? status.State is BlockingState.Yes or BlockingState.Unknown
                               : previousState != status.State);

        _hasCompletedFirstCheck = true;
        if (shouldNotify)
        {
            var message = status.Error is null
                ? status.State == BlockingState.Yes
                    ? $"Se han detectado {status.IpCount} IP."
                    : $"El listado contiene {status.IpCount} IP."
                : Shorten(status.Error, 180);

            ShowBalloon($"Estado: {stateText}", message, StateToolTipIcon(status.State));
        }
    }

    private void ChangeInterval()
    {
        using var dialog = new IntervalDialog(_settings.IntervalMinutes);
        if (dialog.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        var previousInterval = _settings.IntervalMinutes;
        _settings.IntervalMinutes = dialog.IntervalMinutes;
        if (!TrySaveSettings())
        {
            _settings.IntervalMinutes = previousInterval;
            return;
        }

        _timer.Interval = IntervalMilliseconds(_settings.IntervalMinutes);
        UpdateIntervalMenuText();
    }

    private void ToggleNotifications()
    {
        var previousValue = _settings.NotificationsEnabled;
        _settings.NotificationsEnabled = _notificationsItem.Checked;

        if (!TrySaveSettings())
        {
            _settings.NotificationsEnabled = previousValue;
            _notificationsItem.Checked = previousValue;
        }
    }

    private void ToggleStartup()
    {
        try
        {
            _startupManager.SetEnabled(_startupItem.Checked);
        }
        catch (Exception exception)
        {
            _startupItem.Checked = ReadStartupState();
            ShowBalloon("No se pudo cambiar el inicio con Windows", Shorten(exception.Message, 180), ToolTipIcon.Warning);
        }
    }

    private bool TrySaveSettings()
    {
        try
        {
            _settingsStore.Save(_settings);
            return true;
        }
        catch (Exception exception)
        {
            ShowBalloon("No se pudo guardar la configuración", Shorten(exception.Message, 180), ToolTipIcon.Warning);
            return false;
        }
    }

    private bool ReadStartupState()
    {
        try
        {
            return _startupManager.IsEnabled();
        }
        catch
        {
            return false;
        }
    }

    private void UpdateIntervalMenuText() =>
        _intervalItem.Text = $"Cambiar intervalo... ({_settings.IntervalMinutes} min)";

    private void ShowBalloon(string title, string text, ToolTipIcon icon) =>
        _notifyIcon.ShowBalloonTip(5_000, title, text, icon);

    protected override void ExitThreadCore()
    {
        _timer.Stop();
        _notifyIcon.Visible = false;
        _notifyIcon.ContextMenuStrip?.Dispose();
        _notifyIcon.Dispose();
        _timer.Dispose();
        _httpClient.Dispose();
        _yesIcon.Dispose();
        _noIcon.Dispose();
        _unknownIcon.Dispose();
        base.ExitThreadCore();
    }

    private Icon StateIcon(BlockingState state) => state switch
    {
        BlockingState.Yes => _yesIcon,
        BlockingState.No => _noIcon,
        _ => _unknownIcon
    };

    private static string StateText(BlockingState state) => state switch
    {
        BlockingState.Yes => "SI",
        BlockingState.No => "NO",
        _ => "?"
    };

    private static ToolTipIcon StateToolTipIcon(BlockingState state) => state switch
    {
        BlockingState.Yes => ToolTipIcon.Warning,
        BlockingState.No => ToolTipIcon.Info,
        _ => ToolTipIcon.Warning
    };

    private static Version GetCurrentVersion() =>
        Assembly.GetExecutingAssembly().GetName().Version ?? new Version(1, 0, 0);

    private static int IntervalMilliseconds(int minutes) => checked(minutes * 60 * 1000);

    private static string Shorten(string value, int maximumLength) =>
        value.Length <= maximumLength ? value : value[..(maximumLength - 1)] + "…";
}

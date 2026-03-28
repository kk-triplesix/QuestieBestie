using Dalamud.Game.Gui.Dtr;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using ECommons;
using ECommons.DalamudServices;
using QuestieBestie.Services;
using QuestieBestie.UI;

namespace QuestieBestie;

public sealed class QuestieBestiePlugin : IDalamudPlugin, IDisposable
{
    private readonly WindowSystem _windowSystem;
    private readonly MainWindow _mainWindow;
    private readonly OverlayWindow _overlayWindow;
    private readonly DetailWindow _detailWindow;
    private readonly SettingsWindow _settingsWindow;
    private readonly QuestService _questService;
    private readonly TrackingService _trackingService;
    private readonly IDtrBarEntry _dtrEntry;

    public QuestieBestiePlugin(IDalamudPluginInterface pluginInterface)
    {
        ECommonsMain.Init(pluginInterface, this);

        _questService = new QuestService();
        _trackingService = new TrackingService();
        _detailWindow = new DetailWindow(_questService);
        _overlayWindow = new OverlayWindow(_questService, _trackingService);
        _settingsWindow = new SettingsWindow(_trackingService);
        _overlayWindow.SetSettingsWindow(_settingsWindow);
        _mainWindow = new MainWindow(_questService, _detailWindow, _trackingService, _overlayWindow, _settingsWindow);

        _windowSystem = new WindowSystem("QuestieBestie");
        _windowSystem.AddWindow(_mainWindow);
        _windowSystem.AddWindow(_overlayWindow);
        _windowSystem.AddWindow(_detailWindow);
        _windowSystem.AddWindow(_settingsWindow);

        // DTR bar entry
        _dtrEntry = Svc.DtrBar.Get("QuestieBestie");
        _dtrEntry.OnClick += OnDtrClick;
        UpdateDtrText();

        Svc.PluginInterface.UiBuilder.OpenMainUi += OnOpenMainUi;
        Svc.PluginInterface.UiBuilder.OpenConfigUi += OnOpenMainUi;
        Svc.PluginInterface.UiBuilder.Draw += OnDraw;

        Svc.Commands.AddHandler("/questie", new Dalamud.Game.Command.CommandInfo(OnCommand)
        {
            HelpMessage = "Open QuestieBestie window"
        });
    }

    private void OnCommand(string command, string args)
    {
        switch (args.Trim().ToLowerInvariant())
        {
            case "overlay":
                _overlayWindow.Toggle();
                break;
            default:
                _mainWindow.Toggle();
                break;
        }
    }

    private void OnOpenMainUi()
    {
        _mainWindow.Toggle();
    }

    private void OnDtrClick(DtrInteractionEvent e)
    {
        _mainWindow.Toggle();
    }

    private void OnDraw()
    {
        if (Svc.GameGui.GameUiHidden) return;
        _windowSystem.Draw();
        UpdateDtrText();
    }

    private void UpdateDtrText()
    {
        _questService.RefreshCompletionStatus();
        var percent = _questService.CompletionPercent;
        _dtrEntry.Text = $"QB {percent:F0}%";
        _dtrEntry.Tooltip = "QuestieBestie — Click to toggle";
    }

    public void Dispose()
    {
        Svc.Commands.RemoveHandler("/questie");
        Svc.PluginInterface.UiBuilder.OpenMainUi -= OnOpenMainUi;
        Svc.PluginInterface.UiBuilder.OpenConfigUi -= OnOpenMainUi;
        Svc.PluginInterface.UiBuilder.Draw -= OnDraw;
        _dtrEntry.OnClick -= OnDtrClick;
        _dtrEntry.Remove();
        ECommonsMain.Dispose();
    }
}

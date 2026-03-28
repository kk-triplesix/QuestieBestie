using Dalamud.Game.Gui.Dtr;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
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
    private DateTime _lastNotificationCheck = DateTime.MinValue;

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

        // Initialize available quest tracking
        _questService.CheckNewlyAvailable();
    }

    private void OnCommand(string command, string args)
    {
        switch (args.Trim().ToLowerInvariant())
        {
            case "overlay":
                _overlayWindow.Toggle();
                break;
            case "stats":
                _mainWindow.IsOpen = true;
                break;
            default:
                _mainWindow.Toggle();
                break;
        }
    }

    private void OnOpenMainUi() => _mainWindow.Toggle();
    private void OnDtrClick(DtrInteractionEvent e) => _mainWindow.Toggle();

    private void OnDraw()
    {
        if (Svc.GameGui.GameUiHidden) return;
        _windowSystem.Draw();
        UpdateDtrText();
        CheckNotifications();
        AutoRemoveCompleted();
    }

    private void UpdateDtrText()
    {
        _questService.RefreshCompletionStatus();
        var percent = _questService.CompletionPercent;
        _dtrEntry.Text = $"QB {percent:F0}%";
        _dtrEntry.Tooltip = "QuestieBestie — Click to toggle";
    }

    private void CheckNotifications()
    {
        if (!_trackingService.OverlaySettings.ChatNotifications)
            return;

        if ((DateTime.Now - _lastNotificationCheck).TotalSeconds < 10)
            return;

        _lastNotificationCheck = DateTime.Now;

        var newQuests = _questService.CheckNewlyAvailable();
        foreach (var quest in newQuests)
        {
            var msg = new SeStringBuilder()
                .AddUiForeground("[QuestieBestie] ", 35)
                .AddText($"New quest available: ")
                .AddUiForeground(quest.Name, 34)
                .AddText($" (Lv.{quest.RequiredLevel})")
                .Build();

            Svc.Chat.Print(new XivChatEntry { Message = msg, Type = XivChatType.Echo });
        }
    }

    private void AutoRemoveCompleted()
    {
        if (!_trackingService.OverlaySettings.AutoRemoveCompleted)
            return;

        foreach (var list in _trackingService.Lists)
        {
            var toRemove = list.QuestRowIds
                .Where(id => _questService.BlueQuestLookup.TryGetValue(id, out var q) && q.IsCompleted)
                .ToList();

            foreach (var id in toRemove)
                list.QuestRowIds.Remove(id);

            if (toRemove.Count > 0)
                _trackingService.SaveOverlaySettings();
        }
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

using Dalamud.Game.Gui.Dtr;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using QuestieBestie.Services;
using QuestieBestie.UI;

namespace QuestieBestie;

public sealed class QuestieBestiePlugin : IDalamudPlugin, IDisposable
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static IChatGui Chat { get; private set; } = null!;
    [PluginService] internal static ICommandManager Commands { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IDataManager Data { get; private set; } = null!;
    [PluginService] internal static IGameGui GameGui { get; private set; } = null!;
    [PluginService] internal static IDtrBar DtrBar { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;
    [PluginService] internal static IFramework Framework { get; private set; } = null!;

    private readonly WindowSystem _windowSystem;
    private readonly MainWindow _mainWindow;
    private readonly OverlayWindow _overlayWindow;
    private readonly DetailWindow _detailWindow;
    private readonly SettingsWindow _settingsWindow;
    private readonly WidgetWindow _widgetWindow;
    private readonly QuestService _questService;
    private readonly TrackingService _trackingService;
    private readonly IDtrBarEntry _dtrEntry;
    private DateTime _lastNotificationCheck = DateTime.MinValue;
    private float _lastDtrPercent = -1f;

    public QuestieBestiePlugin(IDalamudPluginInterface pluginInterface)
    {
        Loc.Init();

        _questService = new QuestService();
        _trackingService = new TrackingService();
        _questService.SetManuallyCompleted(_trackingService.ManuallyCompleted);
        _detailWindow = new DetailWindow(_questService, _trackingService);
        _settingsWindow = new SettingsWindow(_trackingService, _questService);
        _overlayWindow = new OverlayWindow(_questService, _trackingService, _settingsWindow, _detailWindow);
        _widgetWindow = new WidgetWindow(_questService, _trackingService);
        _mainWindow = new MainWindow(_questService, _detailWindow, _trackingService, _overlayWindow, _settingsWindow, _widgetWindow);

        var currentMax = _questService.BlueQuests.Count > 0 ? _questService.BlueQuests.Max(q => q.RowId) : 0u;
        if (_trackingService.LastKnownMaxRowId == 0)
            _trackingService.LastKnownMaxRowId = currentMax;

        _windowSystem = new WindowSystem("QuestieBestie");
        _windowSystem.AddWindow(_mainWindow);
        _windowSystem.AddWindow(_overlayWindow);
        _windowSystem.AddWindow(_detailWindow);
        _windowSystem.AddWindow(_settingsWindow);
        _windowSystem.AddWindow(_widgetWindow);

        _detailWindow.IsOpen = false;
        _settingsWindow.IsOpen = false;

        _dtrEntry = DtrBar.Get("QuestieBestie");
        _dtrEntry.OnClick += OnDtrClick;
        _dtrEntry.Tooltip = Loc.Get("plugin.dtrTooltip");

        PluginInterface.UiBuilder.OpenMainUi += OnOpenMainUi;
        PluginInterface.UiBuilder.OpenConfigUi += OnOpenConfigUi;
        PluginInterface.UiBuilder.Draw += _windowSystem.Draw;
        Framework.Update += OnFrameworkUpdate;
        Chat.ChatMessage += OnChatMessage;

        Commands.AddHandler("/questie", new Dalamud.Game.Command.CommandInfo(OnCommand)
        {
            HelpMessage = "/questie — Toggle main window | /questie overlay — Toggle overlay | /questie widget — Toggle widget | /questie search <name> — Search quest"
        });

        _questService.CheckNewlyAvailable();
        Log.Information("QuestieBestie loaded — {Count} blue quests found", _questService.BlueQuests.Count);
    }

    private void OnCommand(string command, string args)
    {
        var trimmed = args.Trim();
        var lower = trimmed.ToLowerInvariant();

        switch (lower)
        {
            case "overlay":
                _overlayWindow.Toggle();
                break;
            case "widget":
                _widgetWindow.Toggle();
                break;
            case "stats":
                _mainWindow.IsOpen = true;
                break;
            default:
                if (lower.StartsWith("search "))
                {
                    var searchTerm = trimmed[7..].Trim();
                    SearchAndOpenQuest(searchTerm);
                }
                else
                {
                    _mainWindow.Toggle();
                }
                break;
        }
    }

    private void SearchAndOpenQuest(string searchTerm)
    {
        var match = _questService.BlueQuests.FirstOrDefault(q =>
            q.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));

        if (match != null)
        {
            _detailWindow.ShowQuest(match);
            _questService.OpenQuestOnMap(match.RowId);
        }
        else
        {
            Chat.Print(new XivChatEntry
            {
                Message = new SeStringBuilder()
                    .AddUiForeground("[QuestieBestie] ", 35)
                    .AddText(string.Format(Loc.Get("chat.noMatch"), searchTerm))
                    .Build(),
            });
        }
    }

    private void OnOpenMainUi() => _mainWindow.Toggle();
    private void OnOpenConfigUi() => _settingsWindow.Toggle();
    private void OnDtrClick(DtrInteractionEvent e) => _mainWindow.Toggle();

    private static readonly HashSet<XivChatType> RelevantChatTypes =
    [
        XivChatType.Say, XivChatType.Yell, XivChatType.Shout,
        XivChatType.TellIncoming,
        XivChatType.Party, XivChatType.Alliance,
        XivChatType.FreeCompany,
        XivChatType.Ls1, XivChatType.Ls2, XivChatType.Ls3, XivChatType.Ls4,
        XivChatType.Ls5, XivChatType.Ls6, XivChatType.Ls7, XivChatType.Ls8,
        XivChatType.CrossLinkShell1, XivChatType.CrossLinkShell2, XivChatType.CrossLinkShell3,
        XivChatType.CrossLinkShell4, XivChatType.CrossLinkShell5, XivChatType.CrossLinkShell6,
        XivChatType.CrossLinkShell7, XivChatType.CrossLinkShell8,
    ];

    private void OnChatMessage(Dalamud.Game.Text.XivChatType type, int timestamp, ref SeString sender, ref SeString message, ref bool isHandled)
    {
        if (!RelevantChatTypes.Contains(type))
            return;

        var text = message.TextValue;
        if (!text.Contains("[QuestieBestie]"))
            return;

        var match = _questService.BlueQuests.FirstOrDefault(q => text.Contains(q.Name));
        if (match != null)
            _detailWindow.ShowQuest(match);
    }

    private void OnFrameworkUpdate(IFramework framework)
    {
        if (!ClientState.IsLoggedIn) return;

        _questService.RefreshCompletionStatus();
        UpdateDtrText();
        CheckNotifications();
        AutoRemoveCompleted();
    }

    private void UpdateDtrText()
    {
        var percent = _questService.CompletionPercent;
        if (Math.Abs(percent - _lastDtrPercent) < 0.01f) return;
        _lastDtrPercent = percent;
        _dtrEntry.Text = $"QB {percent:F0}%";
    }

    private void CheckNotifications()
    {
        if (!_trackingService.OverlaySettings.ChatNotifications) return;
        if ((DateTime.Now - _lastNotificationCheck).TotalSeconds < 10) return;
        _lastNotificationCheck = DateTime.Now;

        foreach (var quest in _questService.CheckNewlyAvailable())
        {
            Chat.Print(new XivChatEntry
            {
                Message = new SeStringBuilder()
                    .AddUiForeground("[QuestieBestie] ", 35)
                    .AddText(Loc.Get("chat.newQuest"))
                    .AddUiForeground(quest.Name, 34)
                    .AddText($" (Lv.{quest.RequiredLevel})")
                    .Build(),
            });
        }
    }

    private void AutoRemoveCompleted()
    {
        if (!_trackingService.OverlaySettings.AutoRemoveCompleted) return;

        foreach (var list in _trackingService.Lists)
        {
            var toRemove = list.QuestRowIds
                .Where(id => _questService.BlueQuestLookup.TryGetValue(id, out var q) && q.IsCompleted)
                .ToList();
            foreach (var id in toRemove) list.QuestRowIds.Remove(id);
            if (toRemove.Count > 0) _trackingService.SaveOverlaySettings();
        }
    }

    public void Dispose()
    {
        Framework.Update -= OnFrameworkUpdate;
        Chat.ChatMessage -= OnChatMessage;
        Commands.RemoveHandler("/questie");
        PluginInterface.UiBuilder.OpenMainUi -= OnOpenMainUi;
        PluginInterface.UiBuilder.OpenConfigUi -= OnOpenConfigUi;
        PluginInterface.UiBuilder.Draw -= _windowSystem.Draw;
        _dtrEntry.OnClick -= OnDtrClick;
        _dtrEntry.Remove();

        _windowSystem.RemoveAllWindows();
        _mainWindow.Dispose();
        _overlayWindow.Dispose();
        _detailWindow.Dispose();
        _settingsWindow.Dispose();
        _widgetWindow.Dispose();
    }
}

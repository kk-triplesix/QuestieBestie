using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using ECommons;
using ECommons.DalamudServices;
using QuestieBestie.UI;

namespace QuestieBestie;

public sealed class QuestieBestiePlugin : IDalamudPlugin, IDisposable
{
    private readonly WindowSystem _windowSystem;
    private readonly MainWindow _mainWindow;

    public QuestieBestiePlugin(IDalamudPluginInterface pluginInterface)
    {
        ECommonsMain.Init(pluginInterface, this);

        _mainWindow = new MainWindow();
        _windowSystem = new WindowSystem("QuestieBestie");
        _windowSystem.AddWindow(_mainWindow);

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
        _mainWindow.Toggle();
    }

    private void OnOpenMainUi()
    {
        _mainWindow.Toggle();
    }

    private void OnDraw()
    {
        if (Svc.GameGui.GameUiHidden) return;
        _windowSystem.Draw();
    }

    public void Dispose()
    {
        Svc.Commands.RemoveHandler("/questie");
        Svc.PluginInterface.UiBuilder.OpenMainUi -= OnOpenMainUi;
        Svc.PluginInterface.UiBuilder.OpenConfigUi -= OnOpenMainUi;
        Svc.PluginInterface.UiBuilder.Draw -= OnDraw;
        ECommonsMain.Dispose();
    }
}

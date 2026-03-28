using System.Numerics;
using Dalamud.Interface.Windowing;
using QuestieBestie.Models;
using QuestieBestie.Services;

namespace QuestieBestie.UI;

internal sealed class SettingsWindow : Window
{
    private readonly TrackingService _trackingService;
    private QuestService? _questService;

    public SettingsWindow(TrackingService trackingService)
        : base("QuestieBestie Settings###QuestieBestieSettings", ImGuiWindowFlags.None)
    {
        _trackingService = trackingService;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(420, 600),
            MaximumSize = new Vector2(520, 900),
        };
        IsOpen = false;
    }

    public void SetQuestService(QuestService qs) => _questService = qs;

    public override void PreDraw() => Styles.PushMainStyle();
    public override void PostDraw() => Styles.PopMainStyle();

    public override void Draw()
    {
        var s = _trackingService.OverlaySettings;
        var changed = false;

        ImGui.PushStyleColor(ImGuiCol.Text, Styles.AccentCyan);
        ImGui.Text(Loc.Get("settings.title"));
        ImGui.PopStyleColor();
        ImGui.Separator();
        ImGui.Spacing();

        // General
        ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextSecondary);
        ImGui.Text(Loc.Get("settings.general"));
        ImGui.PopStyleColor();

        changed |= ImGui.SliderFloat("Font Scale", ref s.FontScale, 0.5f, 2.0f, "%.1f");
        changed |= ImGui.SliderFloat("Background Opacity", ref s.BackgroundAlpha, 0.0f, 1.0f, "%.2f");
        changed |= ImGui.SliderFloat("Border Opacity", ref s.BorderAlpha, 0.0f, 1.0f, "%.2f");
        changed |= ImGui.SliderFloat("Window Rounding", ref s.WindowRounding, 0.0f, 20.0f, "%.0f");

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        // Colors
        ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextSecondary);
        ImGui.Text(Loc.Get("settings.colors"));
        ImGui.PopStyleColor();

        changed |= ColorEdit("Text", ref s.TextColor);
        changed |= ColorEdit("Header", ref s.HeaderColor);
        changed |= ColorEdit("Completed", ref s.CompletedColor);
        changed |= ColorEdit("Level Badge", ref s.LevelColor);
        changed |= ColorEdit("Warning", ref s.WarningColor);
        changed |= ColorEdit("Background", ref s.BackgroundColor);
        changed |= ColorEdit("Border", ref s.BorderColor);

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        // Widget Progress Bars
        ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextSecondary);
        ImGui.Text("Widget Progress Bars");
        ImGui.PopStyleColor();

        if (ImGui.Checkbox("Total", ref s.WidgetShowTotal))
            changed = true;

        var expansions = new (uint Id, string Name)[]
        { (0, "A Realm Reborn"), (1, "Heavensward"), (2, "Stormblood"), (3, "Shadowbringers"), (4, "Endwalker"), (5, "Dawntrail") };

        foreach (var (id, name) in expansions)
        {
            var enabled = s.WidgetExpansions.Contains(id);
            ImGui.PushStyleColor(ImGuiCol.Text, Styles.GetExpansionColor(id));
            if (ImGui.Checkbox($"{name}###sExp{id}", ref enabled))
            {
                if (enabled) s.WidgetExpansions.Add(id); else s.WidgetExpansions.Remove(id);
                changed = true;
            }
            ImGui.PopStyleColor();
        }

        // Quest Chains
        if (_questService != null)
        {
            var chains = _questService.BlueQuests
                .Where(q => !string.IsNullOrEmpty(q.ChainName))
                .Select(q => q.ChainName).Distinct().OrderBy(n => n).ToList();

            if (chains.Count > 0)
            {
                ImGui.Spacing();
                ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextSecondary);
                ImGui.Text("Widget Quest Chains");
                ImGui.PopStyleColor();

                foreach (var chainName in chains)
                {
                    var enabled = s.WidgetChains.Contains(chainName);
                    if (ImGui.Checkbox($"{chainName}###sCh{chainName.GetHashCode()}", ref enabled))
                    {
                        if (enabled) s.WidgetChains.Add(chainName); else s.WidgetChains.Remove(chainName);
                        changed = true;
                    }
                }
            }
        }

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        // Behavior
        ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextSecondary);
        ImGui.Text(Loc.Get("settings.behavior"));
        ImGui.PopStyleColor();

        changed |= ImGui.Checkbox(Loc.Get("settings.autoRemove"), ref s.AutoRemoveCompleted);
        changed |= ImGui.Checkbox(Loc.Get("settings.chatNotify"), ref s.ChatNotifications);
        changed |= ImGui.Checkbox(Loc.Get("settings.soundNotify"), ref s.SoundNotifications);

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        if (ImGui.Button(Loc.Get("settings.reset")))
        {
            var defaults = new OverlaySettings();
            s.FontScale = defaults.FontScale;
            s.BackgroundAlpha = defaults.BackgroundAlpha;
            s.BorderAlpha = defaults.BorderAlpha;
            s.WindowRounding = defaults.WindowRounding;
            s.TextColor = defaults.TextColor;
            s.HeaderColor = defaults.HeaderColor;
            s.CompletedColor = defaults.CompletedColor;
            s.LevelColor = defaults.LevelColor;
            s.WarningColor = defaults.WarningColor;
            s.BackgroundColor = defaults.BackgroundColor;
            s.BorderColor = defaults.BorderColor;
            s.WidgetShowTotal = true;
            s.WidgetExpansions.Clear();
            s.WidgetChains.Clear();
            changed = true;
        }

        if (changed)
            _trackingService.SaveOverlaySettings();
    }

    private static bool ColorEdit(string label, ref Vector4 color)
    {
        return ImGui.ColorEdit4(label, ref color, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.AlphaBar);
    }
}

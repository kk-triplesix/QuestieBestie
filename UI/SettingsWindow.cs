using System.Numerics;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using QuestieBestie.Models;
using QuestieBestie.Services;

namespace QuestieBestie.UI;

internal sealed class SettingsWindow : Window
{
    private readonly TrackingService _trackingService;
    private readonly QuestService _questService;
    private bool _syncPopupOpen = true;

    public SettingsWindow(TrackingService trackingService, QuestService questService)
        : base("QuestieBestie Settings###QuestieBestieSettings", ImGuiWindowFlags.None)
    {
        _trackingService = trackingService;
        _questService = questService;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(420, 600) * ImGuiHelpers.GlobalScale,
            MaximumSize = new Vector2(520, 900) * ImGuiHelpers.GlobalScale,
        };
        IsOpen = false;
    }

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

        foreach (var (id, name) in Styles.Expansions)
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

        // Sync with Game State
        ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextSecondary);
        ImGui.Text("Data");
        ImGui.PopStyleColor();

        var manualCount = _trackingService.ManuallyCompleted.Count;
        if (manualCount > 0)
        {
            if (ImGui.Button("Sync with Game State"))
                ImGui.OpenPopup("##confirmSync");

            ImGui.SameLine();
            ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextSecondary);
            ImGui.Text($"({manualCount} manual overrides)");
            ImGui.PopStyleColor();

            using (var modal = ImRaii.PopupModal("##confirmSync", ref _syncPopupOpen, ImGuiWindowFlags.AlwaysAutoResize))
            {
                if (modal.Success)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, Styles.FavoriteStar);
                    ImGui.Text("WARNUNG");
                    ImGui.PopStyleColor();
                    ImGui.Spacing();
                    ImGui.TextWrapped($"Alle {manualCount} manuellen Completion-Aenderungen werden unwiderruflich entfernt und der Quest-Status wird mit dem Gamestate synchronisiert.");
                    ImGui.Spacing();
                    ImGui.TextWrapped("Dieser Vorgang kann nicht rueckgaengig gemacht werden!");
                    ImGui.Spacing();
                    ImGui.Separator();
                    ImGui.Spacing();

                    ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.7f, 0.15f, 0.15f, 1f));
                    ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.9f, 0.2f, 0.2f, 1f));
                    if (ImGui.Button("Ja, alle lokalen Aenderungen entfernen", new Vector2(300 * ImGuiHelpers.GlobalScale, 0)))
                    {
                        _trackingService.ClearManualCompletions();
                        _questService.RefreshCompletionStatus();
                        ImGui.CloseCurrentPopup();
                    }
                    ImGui.PopStyleColor(2);

                    ImGui.SameLine();
                    if (ImGui.Button("Abbrechen", new Vector2(100 * ImGuiHelpers.GlobalScale, 0)))
                        ImGui.CloseCurrentPopup();
                }
            }
        }
        else
        {
            using (ImRaii.Disabled())
            {
                ImGui.Button("Sync with Game State");
            }
            ImGui.SameLine();
            ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextSecondary);
            ImGui.Text("(no manual overrides)");
            ImGui.PopStyleColor();
        }

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

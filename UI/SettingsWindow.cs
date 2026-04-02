using System.Numerics;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using QuestieBestie.Models;
using QuestieBestie.Services;

namespace QuestieBestie.UI;

internal sealed class SettingsWindow : Window, IDisposable
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

    private bool _themePushed;

    public void Dispose() { }

    public override void PreDraw() { _themePushed = _trackingService.OverlaySettings.UseCustomTheme; if (_themePushed) Styles.PushCustomTheme(); }
    public override void PostDraw() { if (_themePushed) Styles.PopCustomTheme(); }

    public override void Draw()
    {
        var s = _trackingService.OverlaySettings;
        var changed = false;

        using (ImRaii.PushColor(ImGuiCol.Text, Styles.AccentCyan))
            ImGui.Text(Loc.Get("settings.title"));
        ImGui.Separator();
        ImGui.Spacing();

        // Theme
        changed |= ImGui.Checkbox(Loc.Get("settings.customTheme"), ref s.UseCustomTheme);

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        // General
        using (ImRaii.PushColor(ImGuiCol.Text, Styles.TextSecondary))
            ImGui.Text(Loc.Get("settings.general"));

        changed |= ImGui.SliderFloat(Loc.Get("settings.fontScale"), ref s.FontScale, 0.5f, 2.0f, "%.1f");
        changed |= ImGui.SliderFloat(Loc.Get("settings.bgOpacity"), ref s.BackgroundAlpha, 0.0f, 1.0f, "%.2f");
        changed |= ImGui.SliderFloat(Loc.Get("settings.borderOpacity"), ref s.BorderAlpha, 0.0f, 1.0f, "%.2f");
        changed |= ImGui.SliderFloat(Loc.Get("settings.windowRounding"), ref s.WindowRounding, 0.0f, 20.0f, "%.0f");

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        // Colors
        using (ImRaii.PushColor(ImGuiCol.Text, Styles.TextSecondary))
            ImGui.Text(Loc.Get("settings.colors"));

        changed |= ColorEdit(Loc.Get("settings.colorText"), ref s.TextColor);
        changed |= ColorEdit(Loc.Get("settings.colorHeader"), ref s.HeaderColor);
        changed |= ColorEdit(Loc.Get("settings.colorCompleted"), ref s.CompletedColor);
        changed |= ColorEdit(Loc.Get("settings.colorLevel"), ref s.LevelColor);
        changed |= ColorEdit(Loc.Get("settings.colorWarning"), ref s.WarningColor);
        changed |= ColorEdit(Loc.Get("settings.colorBg"), ref s.BackgroundColor);
        changed |= ColorEdit(Loc.Get("settings.colorBorder"), ref s.BorderColor);

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        // Widget Progress Bars
        using (ImRaii.PushColor(ImGuiCol.Text, Styles.TextSecondary))
            ImGui.Text(Loc.Get("settings.widgetBars"));

        if (ImGui.Checkbox(Loc.Get("misc.total"), ref s.WidgetShowTotal))
            changed = true;

        foreach (var (id, name) in Styles.Expansions)
        {
            var enabled = s.WidgetExpansions.Contains(id);
            using (ImRaii.PushColor(ImGuiCol.Text, Styles.GetExpansionColor(id)))
                if (ImGui.Checkbox($"{name}###sExp{id}", ref enabled))
                {
                    if (enabled) s.WidgetExpansions.Add(id); else s.WidgetExpansions.Remove(id);
                    changed = true;
                }
        }

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        // Behavior
        using (ImRaii.PushColor(ImGuiCol.Text, Styles.TextSecondary))
            ImGui.Text(Loc.Get("settings.behavior"));

        changed |= ImGui.Checkbox(Loc.Get("settings.autoRemove"), ref s.AutoRemoveCompleted);

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        // Sync with Game State
        using (ImRaii.PushColor(ImGuiCol.Text, Styles.TextSecondary))
            ImGui.Text(Loc.Get("settings.data"));

        var manualCount = _trackingService.ManuallyCompleted.Count;
        if (manualCount > 0)
        {
            if (ImGui.Button(Loc.Get("settings.sync")))
                ImGui.OpenPopup("##confirmSync");

            ImGui.SameLine();
            using (ImRaii.PushColor(ImGuiCol.Text, Styles.TextSecondary))
                ImGui.Text($"({manualCount} {Loc.Get("settings.manualOverrides")})");

            using (var modal = ImRaii.PopupModal("##confirmSync", ref _syncPopupOpen, ImGuiWindowFlags.AlwaysAutoResize))
            {
                if (modal.Success)
                {
                    using (ImRaii.PushColor(ImGuiCol.Text, Styles.FavoriteStar))
                        ImGui.Text(Loc.Get("settings.syncWarning"));
                    ImGui.Spacing();
                    ImGui.TextWrapped(string.Format(Loc.Get("settings.syncDesc"), manualCount));
                    ImGui.Spacing();
                    ImGui.TextWrapped(Loc.Get("settings.syncIrreversible"));
                    ImGui.Spacing();
                    ImGui.Separator();
                    ImGui.Spacing();

                    using (ImRaii.PushColor(ImGuiCol.Button, new Vector4(0.7f, 0.15f, 0.15f, 1f)).Push(ImGuiCol.ButtonHovered, new Vector4(0.9f, 0.2f, 0.2f, 1f)))
                        if (ImGui.Button(Loc.Get("settings.syncConfirm"), new Vector2(300 * ImGuiHelpers.GlobalScale, 0)))
                        {
                            _trackingService.ClearManualCompletions();
                            _questService.RefreshCompletionStatus();
                            ImGui.CloseCurrentPopup();
                        }

                    ImGui.SameLine();
                    if (ImGui.Button(Loc.Get("settings.syncCancel"), new Vector2(100 * ImGuiHelpers.GlobalScale, 0)))
                        ImGui.CloseCurrentPopup();
                }
            }
        }
        else
        {
            using (ImRaii.Disabled())
            {
                ImGui.Button(Loc.Get("settings.sync"));
            }
            ImGui.SameLine();
            using (ImRaii.PushColor(ImGuiCol.Text, Styles.TextSecondary))
                ImGui.Text(Loc.Get("settings.noOverrides"));
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

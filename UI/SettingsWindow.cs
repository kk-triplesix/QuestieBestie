using System.Numerics;
using Dalamud.Interface.Windowing;
using QuestieBestie.Models;
using QuestieBestie.Services;

namespace QuestieBestie.UI;

internal sealed class SettingsWindow : Window
{
    private readonly TrackingService _trackingService;

    public SettingsWindow(TrackingService trackingService)
        : base("QuestieBestie Settings###QuestieBestieSettings", ImGuiWindowFlags.None)
    {
        _trackingService = trackingService;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(400, 450),
            MaximumSize = new Vector2(500, 700),
        };
        IsOpen = false;
    }

    public override void PreDraw()
    {
        Styles.PushMainStyle();
    }

    public override void PostDraw()
    {
        Styles.PopMainStyle();
    }

    public override void Draw()
    {
        var s = _trackingService.OverlaySettings;
        var changed = false;

        ImGui.PushStyleColor(ImGuiCol.Text, Styles.AccentCyan);
        ImGui.Text("Overlay Settings");
        ImGui.PopStyleColor();
        ImGui.Separator();
        ImGui.Spacing();

        // General
        ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextSecondary);
        ImGui.Text("General");
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
        ImGui.Text("Colors");
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

        // Behavior
        ImGui.PushStyleColor(ImGuiCol.Text, Styles.TextSecondary);
        ImGui.Text("Behavior");
        ImGui.PopStyleColor();

        changed |= ImGui.Checkbox("Auto-remove completed quests from lists", ref s.AutoRemoveCompleted);
        changed |= ImGui.Checkbox("Chat notifications for newly available quests", ref s.ChatNotifications);

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        if (ImGui.Button("Reset to Defaults"))
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

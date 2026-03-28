using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility.Raii;
using QuestieBestie.Models;

namespace QuestieBestie.UI;

public static class Icons
{
    public static FontAwesomeIcon GetCategoryIcon(QuestCategory category) => category switch
    {
        QuestCategory.Dungeon => FontAwesomeIcon.Dungeon,
        QuestCategory.Trial => FontAwesomeIcon.Skull,
        QuestCategory.Raid => FontAwesomeIcon.Dragon,
        QuestCategory.JobUnlock => FontAwesomeIcon.LevelUpAlt,
        QuestCategory.Feature => FontAwesomeIcon.Star,
        QuestCategory.Other => FontAwesomeIcon.QuestionCircle,
        _ => FontAwesomeIcon.None,
    };

    public static void DrawIcon(FontAwesomeIcon icon, Vector4? color = null)
    {
        if (color.HasValue) ImGui.PushStyleColor(ImGuiCol.Text, color.Value);
        using (ImRaii.PushFont(UiBuilder.IconFont))
            ImGui.Text(icon.ToIconString());
        if (color.HasValue) ImGui.PopStyleColor();
    }

    public static void DrawIconSameLine(FontAwesomeIcon icon, Vector4? color = null)
    {
        ImGui.SameLine();
        DrawIcon(icon, color);
    }

    public static void DrawCheck(bool completed)
    {
        var icon = completed ? FontAwesomeIcon.Check : FontAwesomeIcon.Times;
        var color = completed ? Styles.TextGreen : Styles.TextSecondary;
        DrawIcon(icon, color);
    }

    public static void DrawFavorite(bool isFav)
    {
        DrawIcon(isFav ? FontAwesomeIcon.Star : FontAwesomeIcon.None,
            isFav ? Styles.FavoriteStar : Styles.TextDimmed);
    }

    public static bool IconButton(FontAwesomeIcon icon, string id, Vector4? color = null)
    {
        if (color.HasValue)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, color.Value);
            var result = ImGuiComponents.IconButton(id, icon);
            ImGui.PopStyleColor();
            return result;
        }
        return ImGuiComponents.IconButton(id, icon);
    }
}

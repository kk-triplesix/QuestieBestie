using Dalamud.Interface.Windowing;

namespace QuestieBestie.UI;

internal class MainWindow : Window
{
    public MainWindow() : base("QuestieBestie###QuestieBestieMain",
        ImGuiWindowFlags.None)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new System.Numerics.Vector2(400, 300),
            MaximumSize = new System.Numerics.Vector2(9999, 9999),
        };
    }

    public override void Draw()
    {
        ImGui.Text("Welcome to QuestieBestie!");
        ImGui.Spacing();
        ImGui.TextWrapped("Your friendly quest companion is ready.");
    }
}

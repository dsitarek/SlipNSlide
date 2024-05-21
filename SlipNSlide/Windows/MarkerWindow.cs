using System;
using System.Drawing;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace SlipNSlide.Windows;

public class MarkerWindow : Window, IDisposable
{
    private Configuration Configuration;
    public Vector2 v1 { get; set; }
    public Vector2 v2 { get; set; }

    // We give this window a constant ID using ###
    // This allows for labels being dynamic, like "{FPS Counter}fps###XYZ counter window",
    // and the window ID will always be "###XYZ counter window" for ImGui
    public MarkerWindow(Plugin plugin) : base("markerwindow###")
    {

        Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBackground;

        Size = new Vector2(232, 75);
        Position = new Vector2(1738, 953);
        SizeCondition = ImGuiCond.Always;

        Configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void PreDraw()
    {
        // Flags must be added or removed before Draw() is being called, or they won't apply
        if (Configuration.IsConfigWindowMovable)
        {
            Flags &= ~ImGuiWindowFlags.NoMove;
        }
        else
        {
            Flags |= ImGuiWindowFlags.NoMove;
        }
    }

    public override void Draw()
    {
        uint ColorToUInt(Color color)
        {
            return (uint)((color.A << 24) | (color.R << 16) |
                          (color.G << 8) | (color.B << 0));
        }
        var color = ColorToUInt(Color.Green);
        ImGui.GetWindowDrawList().AddRectFilled(v1, v2, color);
    }
}

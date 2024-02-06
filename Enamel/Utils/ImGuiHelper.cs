using ImGuiNET;

namespace Enamel.Utils;

public class ImGuiHelper
{
    private float f = 0.0f;

    private bool show_test_window = false;
    private bool show_another_window = false;
    private System.Numerics.Vector3 clear_color = new(114f / 255f, 144f / 255f, 154f / 255f);
    private byte[] _textBuffer = new byte[100];

    public virtual void ImGuiLayout()
    {
        ImGui.Text("All entities with component:");
        ImGui.InputText("Text input", _textBuffer, 100);
        
        ImGui.SliderFloat("float", ref f, 0.0f, 1.0f, string.Empty);
        ImGui.ColorEdit3("clear color", ref clear_color);
        if (ImGui.Button("Test Window")) show_test_window = !show_test_window;
        if (ImGui.Button("Another Window")) show_another_window = !show_another_window;
        ImGui.Text(string.Format("Application average {0:F3} ms/frame ({1:F1} FPS)", 1000f / ImGui.GetIO().Framerate,
            ImGui.GetIO().Framerate));

        // 2. Show another simple window, this time using an explicit Begin/End pair
        if (show_another_window)
        {
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(200, 100), ImGuiCond.FirstUseEver);
            ImGui.Begin("Another Window", ref show_another_window);
            ImGui.Text("Hello");
            ImGui.End();
        }

        // 3. Show the ImGui test window. Most of the sample code is in ImGui.ShowTestWindow()
        if (show_test_window)
        {
            ImGui.SetNextWindowPos(new System.Numerics.Vector2(650, 20), ImGuiCond.FirstUseEver);
            ImGui.ShowDemoWindow(ref show_test_window);
        }
    }
}
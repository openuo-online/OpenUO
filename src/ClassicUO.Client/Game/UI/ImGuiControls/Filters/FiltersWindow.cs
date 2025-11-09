using ImGuiNET;

namespace ClassicUO.Game.UI.ImGuiControls
{
    public class FiltersWindow : SingletonImGuiWindow<FiltersWindow>
    {
        private FiltersWindow() : base(ImGuiTranslations.Get("Filters"))
        {
            WindowFlags = ImGuiWindowFlags.AlwaysAutoResize;
        }

        public override void DrawContent()
        {
            ImGui.Spacing();

            if (ImGui.BeginTabBar("##FilterTabs", ImGuiTabBarFlags.None))
            {
                if (ImGui.BeginTabItem(ImGuiTranslations.Get("Graphics")))
                {
                    GraphicReplacementWindow.GetInstance()?.DrawContent();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem(ImGuiTranslations.Get("Journal Filter")))
                {
                    JournalFilterWindow.GetInstance()?.DrawContent();
                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
            }
        }
    }
}

using ImGuiNET;
using ClassicUO.Configuration;
using ClassicUO.Game.Managers;
using System.Numerics;

namespace ClassicUO.Game.UI.ImGuiControls
{
    public class TitleBarWindow : SingletonImGuiWindow<TitleBarWindow>
    {
        private Profile profile;
        private bool enableTitleBarStats;
        private TitleBarStatsMode statsMode;

        private TitleBarWindow() : base(ImGuiTranslations.Get("Title Bar"))
        {
            WindowFlags = ImGuiWindowFlags.AlwaysAutoResize;
            profile = ProfileManager.CurrentProfile;

            if (profile != null)
            {
                enableTitleBarStats = profile.EnableTitleBarStats;
                statsMode = profile.TitleBarStatsMode;
            }
        }

        public override void DrawContent()
        {
            if (profile == null)
            {
                ImGui.Text(ImGuiTranslations.Get("Profile not loaded"));
                return;
            }

            ImGui.Spacing();

            // Main description
            ImGui.TextWrapped(ImGuiTranslations.Get("Configure window title bar to show HP, Mana, and Stamina information."));
            ImGui.Spacing();

            // Wiki link (placeholder - would need proper link handling)
            if (ImGui.Button(ImGuiTranslations.Get("Title Bar Status Wiki") + "##TitleBarWiki"))
            {
                // Would open wiki link here
                GameActions.Print(Client.Game.UO.World, "Wiki: https://github.com/PlayTazUO/TazUO/wiki/Title-Bar-Status");
            }
            ImGui.Spacing();

            // Enable title bar stats
            if (ImGui.Checkbox(ImGuiTranslations.Get("Enable title bar stats") + "##TitleBarEnable", ref enableTitleBarStats))
            {
                profile.EnableTitleBarStats = enableTitleBarStats;
                if (enableTitleBarStats)
                {
                    TitleBarStatsManager.ForceUpdate();
                }
                else
                {
                    Client.Game.SetWindowTitle(string.IsNullOrEmpty(World.Instance.Player?.Name) ? string.Empty : World.Instance.Player.Name);
                }
            }
            ImGui.Spacing();

            // Display mode section
            ImGui.SeparatorText(ImGuiTranslations.Get("Display Mode:"));
            ImGui.Spacing();

            // Radio buttons for display mode
            if (ImGui.RadioButton(ImGuiTranslations.Get("Text (HP 85/100, MP 42/50, SP 95/100)") + "##TitleBarText", statsMode == TitleBarStatsMode.Text))
            {
                statsMode = TitleBarStatsMode.Text;
                profile.TitleBarStatsMode = statsMode;
                TitleBarStatsManager.ForceUpdate();
            }

            if (ImGui.RadioButton(ImGuiTranslations.Get("Percent (HP 85%, MP 84%, SP 95%)") + "##TitleBarPercent", statsMode == TitleBarStatsMode.Percent))
            {
                statsMode = TitleBarStatsMode.Percent;
                profile.TitleBarStatsMode = statsMode;
                TitleBarStatsManager.ForceUpdate();
            }

            if (ImGui.RadioButton(ImGuiTranslations.Get("Progress Bar (HP [||||||    ] MP [||||||    ] SP [||||||    ])") + "##TitleBarProgress", statsMode == TitleBarStatsMode.ProgressBar))
            {
                statsMode = TitleBarStatsMode.ProgressBar;
                profile.TitleBarStatsMode = statsMode;
                TitleBarStatsManager.ForceUpdate();
            }

            ImGui.Spacing();

            // Preview section
            ImGui.SeparatorText(ImGuiTranslations.Get("Preview:"));
            ImGui.TextWrapped(TitleBarStatsManager.GetPreviewText());
            ImGui.Spacing();

            // Note about Unicode characters
            ImGui.TextWrapped(ImGuiTranslations.Get("Note: Progress bars use Unicode block characters (█▓▒░) and may not display correctly on all systems."));
        }
    }
}

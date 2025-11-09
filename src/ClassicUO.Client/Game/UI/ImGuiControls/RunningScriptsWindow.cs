using ImGuiNET;
using System.Linq;
using System.Numerics;
using ClassicUO.LegionScripting;

namespace ClassicUO.Game.UI.ImGuiControls
{
    public class RunningScriptsWindow : SingletonImGuiWindow<RunningScriptsWindow>
    {
        private RunningScriptsWindow() : base(ImGuiTranslations.Get("Running Scripts"))
        {
            WindowFlags = ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoResize;
        }

        public override void DrawContent()
        {
            System.Collections.Generic.List<ScriptFile> runningScripts = LegionScripting.LegionScripting.RunningScripts;

            if (runningScripts.Count == 0)
            {
                ImGui.TextDisabled(ImGuiTranslations.Get("No scripts currently running"));
            }
            else
            {
                foreach (ScriptFile script in runningScripts.ToArray())
                {
                    if (script == null) continue;

                    ImGui.PushID(script.FullPath?.GetHashCode() ?? 0);

                    if (ImGui.Button(ImGuiTranslations.Get("Stop") + "##StopScript", new Vector2(50, 0)))
                        LegionScripting.LegionScripting.StopScript(script);

                    // Script name on the same line
                    ImGui.SameLine();
                    ImGui.Text(script.FileName ?? ImGuiTranslations.Get("Unknown"));

                    // Show script type as tooltip
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.Text(ImGuiTranslations.Get("Type: ") + script.ScriptType);
                        ImGui.Text(ImGuiTranslations.Get("Path: ") + (script.FullPath ?? ImGuiTranslations.Get("N/A")));
                        ImGui.EndTooltip();
                    }

                    ImGui.PopID();
                }
            }
        }
    }
}

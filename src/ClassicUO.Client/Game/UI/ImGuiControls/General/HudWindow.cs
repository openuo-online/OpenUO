using ImGuiNET;
using ClassicUO.Configuration;
using ClassicUO.Game.Data;
using ClassicUO.Game.Managers;
using ClassicUO.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClassicUO.Game.UI.ImGuiControls
{
    public class HudWindow : SingletonImGuiWindow<HudWindow>
    {
        private Profile profile;
        private Dictionary<HideHudFlags, bool> hudFlagStates;

        private HudWindow() : base(ImGuiTranslations.Get("HUD"))
        {
            WindowFlags = ImGuiWindowFlags.AlwaysAutoResize;
            profile = ProfileManager.CurrentProfile;

            // Initialize HUD flag states
            hudFlagStates = new Dictionary<HideHudFlags, bool>();
            InitializeHudStates();
        }

        private void InitializeHudStates()
        {
            if (profile == null) return;

            foreach (HideHudFlags flag in Enum.GetValues(typeof(HideHudFlags)))
            {
                if (flag == HideHudFlags.None) continue;
                hudFlagStates[flag] = ByteFlagHelper.HasFlag(profile.HideHudGumpFlags, (ulong)flag);
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

            // Header text
            ImGui.TextWrapped(ImGuiTranslations.Get("Check the types of gumps you would like to toggle visibility when using the Toggle Hud Visible macro."));
            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            // Quick actions - use ## to ensure unique IDs
            if (ImGui.Button(ImGuiTranslations.Get("Select All") + "##HudSelectAll"))
            {
                SetAllFlags(true);
            }
            ImGui.SameLine();
            if (ImGui.Button(ImGuiTranslations.Get("Deselect All") + "##HudDeselectAll"))
            {
                SetAllFlags(false);
            }
            ImGui.SameLine();
            if (ImGui.Button(ImGuiTranslations.Get("Toggle HUD Now") + "##HudToggleNow"))
            {
                HideHudManager.ToggleHidden(profile.HideHudGumpFlags);
            }
            ImGuiComponents.Tooltip(ImGuiTranslations.Get("Immediately toggle the visibility of selected HUD elements"));

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            // Draw HUD options in a table format for better organization
            if (ImGui.BeginTable("HudOptionsTable", 2, ImGuiTableFlags.SizingStretchSame))
            {
                ImGui.TableSetupColumn("Column1", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableSetupColumn("Column2", ImGuiTableColumnFlags.WidthStretch);

                bool isFirstColumn = true;
                foreach (KeyValuePair<HideHudFlags, bool> kvp in hudFlagStates.ToList())
                {
                    HideHudFlags flag = kvp.Key;
                    if (flag == HideHudFlags.None) continue;

                    if (isFirstColumn)
                    {
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                    }
                    else
                    {
                        ImGui.TableNextColumn();
                    }

                    bool currentState = kvp.Value;
                    string flagName = HideHudManager.GetFlagName(flag);
                    string translatedFlagName = ImGuiTranslations.Get(flagName);

                    // Use ## to separate display text from ID to avoid conflicts
                    if (ImGui.Checkbox(translatedFlagName + "##Hud" + flag.ToString(), ref currentState))
                    {
                        hudFlagStates[flag] = currentState;
                        UpdateProfileFlags(flag, currentState);

                        // Handle special case for "All" flag
                        if (flag == HideHudFlags.All)
                        {
                            if (currentState)
                            {
                                SetAllFlags(true);
                            }
                            else
                            {
                                // When "All" is unchecked, uncheck everything
                                SetAllFlags(false);
                            }
                        }
                    }

                    // Add tooltip for some flags
                    AddTooltipForFlag(flag);

                    isFirstColumn = !isFirstColumn;
                }

                ImGui.EndTable();
            }

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            // Additional information
            if (ImGui.CollapsingHeader(ImGuiTranslations.Get("Advanced") + "##HudAdvanced"))
            {
                ImGui.Text(ImGuiTranslations.Get("Raw flag value: ") + profile.HideHudGumpFlags);
                if (ImGui.Button(ImGuiTranslations.Get("Reset to Default") + "##HudReset"))
                {
                    profile.HideHudGumpFlags = 0;
                    InitializeHudStates();
                }
                ImGuiComponents.Tooltip(ImGuiTranslations.Get("Reset all HUD visibility settings to default (everything visible)"));
            }
        }

        private void SetAllFlags(bool state)
        {
            foreach (HideHudFlags flag in hudFlagStates.Keys.ToList())
            {
                if (flag == HideHudFlags.None) continue;
                hudFlagStates[flag] = state;
                UpdateProfileFlags(flag, state);
            }
        }

        private void UpdateProfileFlags(HideHudFlags flag, bool enabled)
        {
            if (enabled)
            {
                profile.HideHudGumpFlags = ByteFlagHelper.AddFlag(profile.HideHudGumpFlags, (ulong)flag);
            }
            else
            {
                profile.HideHudGumpFlags = flag == HideHudFlags.All ? (ulong)0 : ByteFlagHelper.RemoveFlag(profile.HideHudGumpFlags, (ulong)flag);
            }
        }

        private void AddTooltipForFlag(HideHudFlags flag)
        {
            string tooltip = flag switch
            {
                HideHudFlags.Paperdoll => ImGuiTranslations.Get("Character paperdoll windows"),
                HideHudFlags.WorldMap => ImGuiTranslations.Get("World map window"),
                HideHudFlags.GridContainers => ImGuiTranslations.Get("Grid-style container windows"),
                HideHudFlags.Containers => ImGuiTranslations.Get("Traditional container windows"),
                HideHudFlags.Healthbars => ImGuiTranslations.Get("Health bar windows"),
                HideHudFlags.StatusBar => ImGuiTranslations.Get("Character status windows"),
                HideHudFlags.SpellBar => ImGuiTranslations.Get("Spell bar windows"),
                HideHudFlags.Journal => ImGuiTranslations.Get("Journal/chat windows"),
                HideHudFlags.XMLGumps => ImGuiTranslations.Get("Server-sent XML gump windows"),
                HideHudFlags.NearbyCorpseLoot => ImGuiTranslations.Get("Nearby corpse loot windows"),
                HideHudFlags.MacroButtons => ImGuiTranslations.Get("Macro button windows"),
                HideHudFlags.SkillButtons => ImGuiTranslations.Get("Skill button windows"),
                HideHudFlags.SkillsMenus => ImGuiTranslations.Get("Skills menu windows"),
                HideHudFlags.TopMenuBar => ImGuiTranslations.Get("Top menu bar"),
                HideHudFlags.DurabilityTracker => ImGuiTranslations.Get("Item durability tracker"),
                HideHudFlags.BuffBar => ImGuiTranslations.Get("Buff/debuff status bars"),
                HideHudFlags.CounterBar => ImGuiTranslations.Get("Item counter bars"),
                HideHudFlags.InfoBar => ImGuiTranslations.Get("Information bars"),
                HideHudFlags.SpellIcons => ImGuiTranslations.Get("Spell icon buttons"),
                HideHudFlags.NameOverheadGump => ImGuiTranslations.Get("Name overhead displays"),
                HideHudFlags.ScriptManagerGump => ImGuiTranslations.Get("Script manager window"),
                HideHudFlags.PlayerChar => ImGuiTranslations.Get("Player character (your avatar in the game world)"),
                HideHudFlags.Mouse => ImGuiTranslations.Get("Mouse cursor"),
                HideHudFlags.All => ImGuiTranslations.Get("Select/deselect all HUD elements at once"),
                _ => null
            };

            if (!string.IsNullOrEmpty(tooltip))
            {
                ImGuiComponents.Tooltip(tooltip);
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            hudFlagStates?.Clear();
        }
    }
}

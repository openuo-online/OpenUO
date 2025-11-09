using ImGuiNET;
using ClassicUO.Configuration;
using ClassicUO.Game.Managers;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.UI.Gumps;
using ClassicUO.Utility;
using System;
using System.IO;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;

namespace ClassicUO.Game.UI.ImGuiControls
{
    public class AutoLootWindow : SingletonImGuiWindow<AutoLootWindow>
    {
        private Profile profile;
        private bool enableAutoLoot;
        private bool enableScavenger;
        private bool enableProgressBar;
        private bool autoLootHumanCorpses;

        private string newGraphicInput = "";
        private string newHueInput = "";
        private string newRegexInput = "";
        private int actionDelay = 1000;

        private List<AutoLootManager.AutoLootConfigEntry> lootEntries;
        private bool showAddEntry = false;
        private Dictionary<string, string> entryGraphicInputs = new Dictionary<string, string>();
        private Dictionary<string, string> entryHueInputs = new Dictionary<string, string>();
        private Dictionary<string, string> entryRegexInputs = new Dictionary<string, string>();
        private Dictionary<string, string> entryDestinationInputs = new Dictionary<string, string>();
        private bool showCharacterImportPopup = false;

        private AutoLootWindow() : base(ImGuiTranslations.Get("Auto Loot"))
        {
            WindowFlags = ImGuiWindowFlags.AlwaysAutoResize;
            profile = ProfileManager.CurrentProfile;

            enableAutoLoot = profile.EnableAutoLoot;
            enableScavenger = profile.EnableScavenger;
            enableProgressBar = profile.EnableAutoLootProgressBar;
            autoLootHumanCorpses = profile.AutoLootHumanCorpses;
            actionDelay = profile.MoveMultiObjectDelay;

            lootEntries = AutoLootManager.Instance.AutoLootList;
        }

        public override void DrawContent()
        {
            if (profile == null)
            {
                ImGui.Text(ImGuiTranslations.Get("Profile not loaded"));
                return;
            }
            // Main settings
            ImGui.Spacing();
            if (ImGui.Checkbox(ImGuiTranslations.Get("Enable Auto Loot") + "##AutoLootEnable", ref enableAutoLoot))
            {
                profile.EnableAutoLoot = enableAutoLoot;
            }
            ImGuiComponents.Tooltip(ImGuiTranslations.Get("Auto Loot allows you to automatically pick up items from corpses based on configured criteria."));

            ImGui.SameLine();

            if (ImGui.Button(ImGuiTranslations.Get("Set Grab Bag") + "##AutoLootSetBag"))
            {
                GameActions.Print(Client.Game.UO.World, ImGuiTranslations.Get("Target container to grab items into"));
                Client.Game.UO.World.TargetManager.SetTargeting(CursorTarget.SetGrabBag, 0, TargetType.Neutral);
            }
            ImGui.SameLine();

            ImGuiComponents.Tooltip(ImGuiTranslations.Get("Choose a container to grab items into"));

            ImGui.SeparatorText(ImGuiTranslations.Get("Options:"));

            if (ImGui.Checkbox(ImGuiTranslations.Get("Enable Scavenger") + "##AutoLootScavenger", ref enableScavenger))
            {
                profile.EnableScavenger = enableScavenger;
            }
            ImGui.SameLine();

            ImGuiComponents.Tooltip(ImGuiTranslations.Get("Scavenger option allows to pick objects from ground."));

            if (ImGui.Checkbox(ImGuiTranslations.Get("Enable progress bar") + "##AutoLootProgressBar", ref enableProgressBar))
            {
                profile.EnableAutoLootProgressBar = enableProgressBar;
            }
            ImGui.SameLine();

            ImGuiComponents.Tooltip(ImGuiTranslations.Get("Shows a progress bar gump."));


            if (ImGui.Checkbox(ImGuiTranslations.Get("Auto loot human corpses") + "##AutoLootHuman", ref autoLootHumanCorpses))
            {
                profile.AutoLootHumanCorpses = autoLootHumanCorpses;
            }
            ImGui.SameLine();

            ImGuiComponents.Tooltip(ImGuiTranslations.Get("Auto loots human corpses."));

            // Buttons for grab bag and import/export
            ImGui.SeparatorText(ImGuiTranslations.Get("Import & Export:"));
            if (ImGui.Button(ImGuiTranslations.Get("Export JSON") + "##AutoLootExport"))
            {
                FileSelector.ShowFileBrowser(Client.Game.UO.World, FileSelectorType.Directory, null, null, (selectedPath) =>
                {
                    if (string.IsNullOrWhiteSpace(selectedPath)) return;
                    string fileName = $"AutoLoot_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                    string fullPath = Path.Combine(selectedPath, fileName);
                    AutoLootManager.Instance.ExportToFile(fullPath);
                }, "Export Autoloot Configuration");
            }

            ImGui.SameLine();
            if (ImGui.Button(ImGuiTranslations.Get("Import JSON") + "##AutoLootImport"))
            {
                FileSelector.ShowFileBrowser(Client.Game.UO.World, FileSelectorType.File, null, new[] { "json" }, (selectedFile) =>
                {
                    if (string.IsNullOrWhiteSpace(selectedFile)) return;
                    AutoLootManager.Instance.ImportFromFile(selectedFile);
                    // Clear input dictionaries to refresh with new data
                    entryGraphicInputs.Clear();
                    entryHueInputs.Clear();
                    entryRegexInputs.Clear();
                    entryDestinationInputs.Clear();
                    lootEntries = AutoLootManager.Instance.AutoLootList;
                }, "Import Autoloot Configuration");
            }

            ImGui.SameLine();
            if (ImGui.Button(ImGuiTranslations.Get("Import from Character") + "##AutoLootImportChar"))
            {
                showCharacterImportPopup = true;
            }

            // Add entry section
            ImGui.SeparatorText(ImGuiTranslations.Get("Entries:"));

            if (ImGui.Button(ImGuiTranslations.Get("Add Manual Entry") + "##AutoLootAddManual"))
            {
                showAddEntry = !showAddEntry;
            }
            ImGui.SameLine();
            if (ImGui.Button(ImGuiTranslations.Get("Add from Target") + "##AutoLootAddTarget"))
            {
                World.Instance.TargetManager.SetTargeting((targetedItem) =>
                {
                    if (targetedItem != null && targetedItem is Entity targetedEntity)
                    {
                        if (SerialHelper.IsItem(targetedEntity))
                        {
                            AutoLootManager.Instance.AddAutoLootEntry(targetedEntity.Graphic, targetedEntity.Hue, targetedEntity.Name);
                            lootEntries = AutoLootManager.Instance.AutoLootList;
                        }
                    }
                });
            }

            if (showAddEntry)
            {
                ImGui.SeparatorText(ImGuiTranslations.Get("Add New Entry:"));
                ImGui.Spacing();

                ImGui.BeginGroup();
                ImGui.AlignTextToFramePadding();
                ImGui.Text(ImGuiTranslations.Get("Graphic:"));
                ImGui.SameLine();
                ImGuiComponents.Tooltip(ImGuiTranslations.Get("Item Graphic"));
                ImGui.SetNextItemWidth(70);
                ImGui.InputText("##NewGraphic", ref newGraphicInput, 10);
                ImGui.EndGroup();

                ImGui.SameLine();

                ImGui.BeginGroup();
                ImGui.AlignTextToFramePadding();
                ImGui.Text(ImGuiTranslations.Get("Hue:"));
                ImGui.SameLine();

                ImGuiComponents.Tooltip(ImGuiTranslations.Get("Set -1 to match any Hue"));
                ImGui.SetNextItemWidth(70);
                ImGui.InputText("##NewHue", ref newHueInput, 10);
                ImGui.EndGroup();

                ImGui.Text(ImGuiTranslations.Get("Regex:"));
                ImGui.InputText("##NewRegex", ref newRegexInput, 500);

                ImGui.Spacing();

                if (ImGui.Button(ImGuiTranslations.Get("Add") + "##AddEntry"))
                {
                    if (StringHelper.TryParseGraphic(newGraphicInput, out int graphic))
                    {
                        ushort hue = ushort.MaxValue;
                        if (!string.IsNullOrEmpty(newHueInput) && newHueInput != "-1")
                        {
                            ushort.TryParse(newHueInput, out hue);
                        }

                        AutoLootManager.AutoLootConfigEntry entry = AutoLootManager.Instance.AddAutoLootEntry((ushort)graphic, hue, "");
                        entry.RegexSearch = newRegexInput;

                        newGraphicInput = "";
                        newHueInput = "";
                        newRegexInput = "";
                        showAddEntry = false;
                        lootEntries = AutoLootManager.Instance.AutoLootList;
                    }
                }
                ImGui.SameLine();
                if (ImGui.Button(ImGuiTranslations.Get("Cancel") + "##AddEntryCancel"))
                {
                    showAddEntry = false;
                    newGraphicInput = "";
                    newHueInput = "";
                    newRegexInput = "";
                }
            }

            ImGui.SeparatorText(ImGuiTranslations.Get("Current Auto Loot Entries:"));
            // List of current entries

            if (lootEntries.Count == 0)
            {
                ImGui.Text(ImGuiTranslations.Get("No entries configured"));
            }
            else
            // Table headers
            if (ImGui.BeginTable("AutoLootTable", 6, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY, new Vector2(0, ImGuiTheme.Dimensions.STANDARD_TABLE_SCROLL_HEIGHT)))
            {
                ImGui.TableSetupColumn(string.Empty, ImGuiTableColumnFlags.WidthFixed, 52);
                ImGui.TableSetupColumn(ImGuiTranslations.Get("Graphic"), ImGuiTableColumnFlags.WidthFixed, ImGuiTheme.Dimensions.STANDARD_INPUT_WIDTH);
                ImGui.TableSetupColumn(ImGuiTranslations.Get("Hue"), ImGuiTableColumnFlags.WidthFixed, ImGuiTheme.Dimensions.STANDARD_INPUT_WIDTH);
                ImGui.TableSetupColumn(ImGuiTranslations.Get("Regex"), ImGuiTableColumnFlags.WidthFixed, 60);
                ImGui.TableSetupColumn(ImGuiTranslations.Get("Destination"), ImGuiTableColumnFlags.WidthFixed, 150);
                ImGui.TableSetupColumn(ImGuiTranslations.Get("Actions"), ImGuiTableColumnFlags.WidthFixed, 60);
                ImGui.TableHeadersRow();

                for (int i = lootEntries.Count - 1; i >= 0; i--)
                {
                    AutoLootManager.AutoLootConfigEntry entry = lootEntries[i];
                    ImGui.TableNextRow();

                    ImGui.TableNextColumn();
                    if (!DrawArt((ushort)entry.Graphic, new Vector2(50, 50)))
                        ImGui.Text($"{entry.Graphic:X4}");
                    SetTooltip(entry.Name);

                    ImGui.TableNextColumn();
                    // Initialize input string if not exists
                    if (!entryGraphicInputs.ContainsKey(entry.Uid))
                    {
                        entryGraphicInputs[entry.Uid] = entry.Graphic.ToString();
                    }
                    string graphicStr = entryGraphicInputs[entry.Uid];
                    if (ImGui.InputText($"##Graphic{i}", ref graphicStr, 10))
                    {
                        entryGraphicInputs[entry.Uid] = graphicStr;
                        if (StringHelper.TryParseGraphic(graphicStr, out int newGraphic))
                        {
                            entry.Graphic = newGraphic;
                        }
                    }
                    SetTooltip("Set to -1 to match any graphic.");

                    ImGui.TableNextColumn();
                    // Initialize input string if not exists
                    if (!entryHueInputs.ContainsKey(entry.Uid))
                    {
                        entryHueInputs[entry.Uid] = entry.Hue == ushort.MaxValue ? "-1" : entry.Hue.ToString();
                    }
                    string hueStr = entryHueInputs[entry.Uid];
                    if (ImGui.InputText($"##Hue{i}", ref hueStr, 10))
                    {
                        entryHueInputs[entry.Uid] = hueStr;
                        if (hueStr == "-1")
                        {
                            entry.Hue = ushort.MaxValue;
                        }
                        else if (ushort.TryParse(hueStr, out ushort newHue))
                        {
                            entry.Hue = newHue;
                        }
                    }
                    SetTooltip("Set to -1 to match any hue.");

                    ImGui.TableNextColumn();
                    // Initialize input string if not exists
                    if (!entryRegexInputs.ContainsKey(entry.Uid))
                    {
                        entryRegexInputs[entry.Uid] = entry.RegexSearch ?? "";
                    }
                    string regexStr = entryRegexInputs[entry.Uid];


                    if (ImGui.Button($"Edit##{i}"))
                    {
                        ImGui.OpenPopup($"RegexEditor##{i}");
                    }

                    if (ImGui.BeginPopup($"RegexEditor##{i}"))
                    {
                        ImGui.TextColored(ImGuiTheme.Current.Primary, "Regex Editor:");

                        if (ImGui.InputTextMultiline($"##Regex{i}", ref regexStr, 500, new Vector2(300, 100)))
                        {
                            entryRegexInputs[entry.Uid] = regexStr;
                            entry.RegexSearch = regexStr;
                        }

                        if (ImGui.Button("Close"))
                            ImGui.CloseCurrentPopup();

                        ImGui.EndPopup();
                    }

                    ImGui.TableNextColumn();
                    // Initialize input string if not exists
                    if (!entryDestinationInputs.ContainsKey(entry.Uid))
                    {
                        entryDestinationInputs[entry.Uid] = entry.DestinationContainer == 0 ? "" : $"0x{entry.DestinationContainer:X}";
                    }
                    string destStr = entryDestinationInputs[entry.Uid];
                    ImGui.SetNextItemWidth(80);
                    if (ImGui.InputText($"##Dest{i}", ref destStr, 20))
                    {
                        entryDestinationInputs[entry.Uid] = destStr;
                        if (string.IsNullOrWhiteSpace(destStr))
                        {
                            entry.DestinationContainer = 0;
                        }
                        else if (uint.TryParse(destStr.Replace("0x", "").Replace("0X", ""), System.Globalization.NumberStyles.HexNumber, null, out uint destSerial))
                        {
                            entry.DestinationContainer = destSerial;
                        }
                    }
                    ImGui.SameLine();
                    if (ImGui.Button($"Target##Dest{i}"))
                    {
                        World.Instance.TargetManager.SetTargeting((targetedContainer) =>
                        {
                            if (targetedContainer != null && targetedContainer is Entity targetedEntity)
                            {
                                if (SerialHelper.IsItem(targetedEntity))
                                {
                                    entry.DestinationContainer = targetedEntity.Serial;
                                    entryDestinationInputs[entry.Uid] = $"0x{targetedEntity.Serial:X}";
                                }
                            }
                        });
                    }

                    ImGui.TableNextColumn();
                    if (ImGui.Button($"Delete##Delete{i}"))
                    {
                        AutoLootManager.Instance.TryRemoveAutoLootEntry(entry.Uid);
                        // Clean up input dictionaries
                        entryGraphicInputs.Remove(entry.Uid);
                        entryHueInputs.Remove(entry.Uid);
                        entryRegexInputs.Remove(entry.Uid);
                        entryDestinationInputs.Remove(entry.Uid);
                        lootEntries = AutoLootManager.Instance.AutoLootList;
                    }
                }

                ImGui.EndTable();
            }

            // Character import popup
            if (showCharacterImportPopup)
            {
                ImGui.OpenPopup("Import from Character");
                showCharacterImportPopup = false;
            }

            if (ImGui.BeginPopupModal("Import from Character"))
            {
                Dictionary<string, List<AutoLootManager.AutoLootConfigEntry>> otherConfigs = AutoLootManager.Instance.GetOtherCharacterConfigs();

                if (otherConfigs.Count == 0)
                {
                    ImGui.Text("No other character autoloot configurations found.");
                    if (ImGui.Button("OK"))
                    {
                        ImGui.CloseCurrentPopup();
                    }
                }
                else
                {
                    ImGui.Text("Select a character to import autoloot configuration from:");
                    ImGui.Separator();

                    foreach (KeyValuePair<string, List<AutoLootManager.AutoLootConfigEntry>> characterConfig in otherConfigs.OrderBy(c => c.Key))
                    {
                        string characterName = characterConfig.Key;
                        List<AutoLootManager.AutoLootConfigEntry> configs = characterConfig.Value;

                        if (ImGui.Button($"{characterName} ({configs.Count} items)"))
                        {
                            AutoLootManager.Instance.ImportFromOtherCharacter(characterName, configs);
                            // Clear input dictionaries to refresh with new data
                            entryGraphicInputs.Clear();
                            entryHueInputs.Clear();
                            entryRegexInputs.Clear();
                            entryDestinationInputs.Clear();
                            lootEntries = AutoLootManager.Instance.AutoLootList;
                            ImGui.CloseCurrentPopup();
                        }
                    }

                    ImGui.Separator();
                    if (ImGui.Button("Cancel"))
                    {
                        ImGui.CloseCurrentPopup();
                    }
                }

                ImGui.EndPopup();
            }
        }


    }
}

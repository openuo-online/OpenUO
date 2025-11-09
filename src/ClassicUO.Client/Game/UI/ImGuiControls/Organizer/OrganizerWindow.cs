using ClassicUO.Game.Managers;
using ClassicUO.Game.GameObjects;
using ImGuiNET;
using System;
using System.Linq;
using System.Numerics;

namespace ClassicUO.Game.UI.ImGuiControls
{
    public class OrganizerWindow : SingletonImGuiWindow<OrganizerWindow>
    {
        private int _selectedConfigIndex = -1;
        private OrganizerConfig _selectedConfig = null;
        private string _addItemGraphicInput = "";
        private string _addItemHueInput = "";
        private bool _showAddItemManual = false;

        private OrganizerWindow() : base(ImGuiTranslations.Get("Organizer"))
        {
            WindowFlags = ImGuiWindowFlags.AlwaysAutoResize;
        }

        public override void DrawContent()
        {
            if (OrganizerAgent.Instance == null)
            {
                ImGui.Text(ImGuiTranslations.Get("Profile not loaded"));
                return;
            }

            // Main layout: left panel for organizer list, right panel for details
            if (ImGui.BeginTable("OrganizerTable", 2, ImGuiTableFlags.Resizable))
            {
                ImGui.TableSetupColumn(ImGuiTranslations.Get("Organizers"), ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn(ImGuiTranslations.Get("Details"), ImGuiTableColumnFlags.WidthStretch);

                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                DrawOrganizerList();

                ImGui.TableSetColumnIndex(1);
                DrawOrganizerDetails();

                ImGui.EndTable();
            }
        }

        private void DrawOrganizerList()
        {

            ImGui.Separator();
            if (ImGui.Button(ImGuiTranslations.Get("Add Organizer") + "##AddOrganizer"))
            {
                OrganizerConfig newConfig = OrganizerAgent.Instance.NewOrganizerConfig();
                _selectedConfigIndex = OrganizerAgent.Instance.OrganizerConfigs.IndexOf(newConfig);
                _selectedConfig = newConfig;
            }

            ImGui.SeparatorText(ImGuiTranslations.Get("List"));

            // List existing organizers
            System.Collections.Generic.List<OrganizerConfig> configs = OrganizerAgent.Instance.OrganizerConfigs;
            for (int i = 0; i < configs.Count; i++)
            {
                OrganizerConfig config = configs[i];
                bool isSelected = i == _selectedConfigIndex;

                string label = $"{config.Name}##Config{i}";
                if (config.Enabled)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, new System.Numerics.Vector4(0.0f, 1.0f, 0.0f, 1.0f));
                }

                if (ImGui.Selectable(label, isSelected))
                {
                    _selectedConfigIndex = i;
                    _selectedConfig = config;
                }

                if (config.Enabled)
                {
                    ImGui.PopStyleColor();
                }

                // Show item count as tooltip
                if (ImGui.IsItemHovered())
                {
                    int enabledItems = config.ItemConfigs.Count(ic => ic.Enabled);
                    ImGui.SetTooltip($"{enabledItems} {ImGuiTranslations.Get("enabled items")}");
                }
            }
        }

        private void DrawOrganizerDetails()
        {
            if (_selectedConfig == null || _selectedConfigIndex == -1)
            {
                ImGui.Text(ImGuiTranslations.Get("Select an organizer to view details"));
                return;
            }


            bool enabled = _selectedConfig.Enabled;
            if (ImGui.Checkbox(ImGuiTranslations.Get("Enabled") + "##OrgEnabled", ref enabled))
                _selectedConfig.Enabled = enabled;

            string name = _selectedConfig.Name;
            ImGui.SeparatorText(ImGuiTranslations.Get("Options:"));

            ImGui.SetNextItemWidth(150);
            if (ImGui.InputText(ImGuiTranslations.Get("Name") + "##OrgName", ref name, 100))
            {
                _selectedConfig.Name = name;
            }




            // Action buttons
            if (ImGui.Button(ImGuiTranslations.Get("Run Organizer") + "##RunOrg"))
            {
                OrganizerAgent.Instance?.RunOrganizer(_selectedConfig.Name);
            }

            ImGui.SameLine();
            if (ImGui.Button(ImGuiTranslations.Get("Duplicate") + "##DupeOrg"))
            {
                OrganizerConfig duplicated = OrganizerAgent.Instance?.DupeConfig(_selectedConfig);
                if (duplicated != null)
                {
                    _selectedConfigIndex = OrganizerAgent.Instance.OrganizerConfigs.IndexOf(duplicated);
                    _selectedConfig = duplicated;
                }
            }

            ImGui.SameLine();

            if (ImGui.Button(ImGuiTranslations.Get("Create Macro") + "##CreateOrgMacro"))
            {
                OrganizerAgent.Instance?.CreateOrganizerMacroButton(_selectedConfig.Name);
                GameActions.Print(ImGuiTranslations.Get("Created Organizer Macro: ") + _selectedConfig.Name);
            }

            ImGui.SameLine();
            ImGui.PushStyleColor(ImGuiCol.Button, new System.Numerics.Vector4(0.8f, 0.2f, 0.2f, 1.0f));
            if (ImGui.Button(ImGuiTranslations.Get("Delete") + "##DeleteOrg"))
            {
                OrganizerAgent.Instance?.DeleteConfig(_selectedConfig);
                //_selectedConfig = null;
                _selectedConfigIndex = -1;
            }
            ImGui.PopStyleColor();

            // Container settings
            DrawContainerSettings();

            ImGui.NewLine();

            // Items section
            DrawItemsSection();
        }

        private void DrawContainerSettings()
        {
            ImGui.SeparatorText(ImGuiTranslations.Get("Container Settings:"));

            if (ImGui.Button(ImGuiTranslations.Get("Set Source Container") + "##SetSrcCont"))
            {
                GameActions.Print(ImGuiTranslations.Get("Select [SOURCE] Container"), 82);
                World.Instance.TargetManager.SetTargeting((source) =>
                {
                    if (source == null || !(source is Entity sourceEntity) || !SerialHelper.IsItem(sourceEntity))
                    {
                        GameActions.Print(ImGuiTranslations.Get("Only items can be selected!"));
                        return;
                    }
                    _selectedConfig.SourceContSerial = sourceEntity.Serial;
                    GameActions.Print(ImGuiTranslations.Get("Source container set to ") + $"{sourceEntity.Serial:X}", 63);
                });
            }

            ImGui.SameLine();
            if (ImGui.Button(ImGuiTranslations.Get("Set Destination Container") + "##SetDestCont"))
            {
                GameActions.Print(ImGuiTranslations.Get("Select [DESTINATION] Container"), 82);
                World.Instance.TargetManager.SetTargeting((destination) =>
                {
                    if (destination == null || !(destination is Entity destEntity) || !SerialHelper.IsItem(destEntity))
                    {
                        GameActions.Print(ImGuiTranslations.Get("Only items can be selected!"));
                        return;
                    }
                    _selectedConfig.DestContSerial = destEntity.Serial;
                    GameActions.Print(ImGuiTranslations.Get("Destination container set to ") + $"{destEntity.Serial:X}", 63);
                });
            }

            // Display current containers
            if (_selectedConfig.SourceContSerial != 0)
            {
                ImGui.Text(ImGuiTranslations.Get("Source: ") + $"({_selectedConfig.SourceContSerial:X})");
            }
            else
            {
                ImGui.TextColored(new System.Numerics.Vector4(0.0f, 1.0f, 0.0f, 1.0f), ImGuiTranslations.Get("Source: Your backpack"));
            }

            ImGui.SameLine();

            if (_selectedConfig.DestContSerial != 0)
            {
                ImGui.Text(ImGuiTranslations.Get("Destination: ") + $"({_selectedConfig.DestContSerial:X})");
            }
            else
            {
                ImGui.Text(ImGuiTranslations.Get("Destination: Not set"));
            }
        }

        private void DrawItemsSection()
        {
            ImGui.SeparatorText(ImGuiTranslations.Get("Items to Organize:"));
            ImGui.AlignTextToFramePadding();
            // Add item buttons
            if (ImGui.Button(ImGuiTranslations.Get("Target Item to Add") + "##TargetAddItem"))
            {
                World.Instance.TargetManager.SetTargeting((obj) =>
                {
                    if (obj == null || !(obj is Entity objEntity) || !SerialHelper.IsItem(objEntity))
                    {
                        GameActions.Print(ImGuiTranslations.Get("Only items can be added!"));
                        return;
                    }

                    OrganizerItemConfig newItemConfig = _selectedConfig.NewItemConfig();
                    newItemConfig.Graphic = objEntity.Graphic;
                    newItemConfig.Hue = objEntity.Hue;

                    GameActions.Print(ImGuiTranslations.Get("Added item: Graphic ") + $"{objEntity.Graphic:X}, Hue {objEntity.Hue:X}");
                });
            }

            ImGui.SameLine();
            if (ImGui.Button(ImGuiTranslations.Get("Add Item Manually") + "##ManualAddItem"))
            {
                _showAddItemManual = !_showAddItemManual;
            }

            // Manual add item section
            if (_showAddItemManual)
            {
                ImGui.SeparatorText(ImGuiTranslations.Get("Manual Entry:"));
                ImGui.Text(ImGuiTranslations.Get("Graphic:"));
                ImGuiComponents.Tooltip(ImGuiTranslations.Get("Hex value, e.g. 0x0EED."));
                ImGui.SetNextItemWidth(110);
                ImGui.InputText("##graphic", ref _addItemGraphicInput, 10);
                ImGui.Text(ImGuiTranslations.Get("Hue:"));
                ImGuiComponents.Tooltip(ImGuiTranslations.Get("Set to -1 to match any hue."));
                ImGui.SetNextItemWidth(110);
                ImGui.InputText("##hue", ref _addItemHueInput, 10);
                if (ImGui.Button(ImGuiTranslations.Get("Add") + "##AddManualItem"))
                {
                    if (ushort.TryParse(_addItemGraphicInput, System.Globalization.NumberStyles.HexNumber, null, out ushort graphic))
                    {
                        OrganizerItemConfig newItemConfig = _selectedConfig.NewItemConfig();
                        newItemConfig.Graphic = graphic;

                        if (int.TryParse(_addItemHueInput, out int hue) && hue >= -1)
                        {
                            newItemConfig.Hue = hue == -1 ? ushort.MaxValue : (ushort)hue;
                        }

                        _addItemGraphicInput = "";
                        _addItemHueInput = "";
                        _showAddItemManual = false;
                    }
                }
                ImGui.SameLine();
                if (ImGui.Button(ImGuiTranslations.Get("Cancel") + "##CancelManualAdd"))
                {
                    _addItemGraphicInput = "";
                    _addItemHueInput = "";
                    _showAddItemManual = false;
                }
            }

            // Items table
            if (ImGui.BeginTable("ItemsTable", 6, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY, new Vector2(0, ImGuiTheme.Dimensions.STANDARD_TABLE_SCROLL_HEIGHT)))
            {
                ImGui.TableSetupColumn(ImGuiTranslations.Get("Graphic"), ImGuiTableColumnFlags.WidthFixed, 55);
                ImGui.TableSetupColumn(ImGuiTranslations.Get("Hue"), ImGuiTableColumnFlags.WidthFixed, ImGuiTheme.Dimensions.STANDARD_INPUT_WIDTH);
                ImGui.TableSetupColumn(ImGuiTranslations.Get("Amount"), ImGuiTableColumnFlags.WidthFixed, ImGuiTheme.Dimensions.STANDARD_INPUT_WIDTH);
                ImGui.TableSetupColumn(ImGuiTranslations.Get("Destination"), ImGuiTableColumnFlags.WidthFixed, 120);
                ImGui.TableSetupColumn(ImGuiTranslations.Get("Enabled"), ImGuiTableColumnFlags.WidthFixed, ImGuiTheme.Dimensions.STANDARD_INPUT_WIDTH);
                ImGui.TableSetupColumn("Del", ImGuiTableColumnFlags.WidthFixed, ImGuiTheme.Dimensions.STANDARD_INPUT_WIDTH);
                ImGui.TableHeadersRow();

                for (int i = _selectedConfig.ItemConfigs.Count - 1; i >= 0; i--)
                {
                    OrganizerItemConfig itemConfig = _selectedConfig.ItemConfigs[i];
                    ImGui.TableNextRow();

                    ImGui.TableSetColumnIndex(0);
                    if (!DrawArt(itemConfig.Graphic, new Vector2(50, 50)))
                        ImGui.Text($"{itemConfig.Graphic:X4}");

                    ImGui.TableSetColumnIndex(1);
                    string hueText = itemConfig.Hue == ushort.MaxValue ? ImGuiTranslations.Get("ANY") : itemConfig.Hue.ToString();
                    if (ImGui.InputText($"##Hue{i}", ref hueText, 5))
                    {
                        if (ushort.TryParse(hueText, System.Globalization.NumberStyles.HexNumber, null, out ushort hue))
                            itemConfig.Hue = hue == 0xFFFF ? ushort.MaxValue : hue;
                        else if (hueText == ImGuiTranslations.Get("ANY"))
                            itemConfig.Hue = ushort.MaxValue;
                    }

                    SetTooltip(ImGuiTranslations.Get("Set to -1 to match any hue."));

                    ImGui.TableSetColumnIndex(2);
                    int amount = itemConfig.Amount;
                    if (ImGui.InputInt($"##Amount{i}", ref amount, 0, 0))
                    {
                        itemConfig.Amount = (ushort)Math.Max(0, Math.Min(65535, amount));
                    }

                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip(ImGuiTranslations.Get("This takes into account the items in the destination container.\n(0 = move all items)"));
                    }

                    ImGui.TableSetColumnIndex(3);
                    // Per-item destination
                    if (itemConfig.DestContSerial != 0)
                    {
                        ImGui.Text($"{itemConfig.DestContSerial:X}");
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.SetTooltip(ImGuiTranslations.Get("Per-item destination"));
                        }
                        ImGui.SameLine();
                        if (ImGui.SmallButton($"X##ClearDest{i}"))
                        {
                            itemConfig.DestContSerial = 0;
                        }
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.SetTooltip(ImGuiTranslations.Get("Clear and use config destination"));
                        }
                    }
                    else
                    {
                        ImGui.TextColored(new Vector4(0.5f, 0.5f, 0.5f, 1.0f), ImGuiTranslations.Get("Config"));
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.SetTooltip(ImGuiTranslations.Get("Using configuration's destination"));
                        }
                        ImGui.SameLine();
                        if (ImGui.SmallButton($"Set##SetDest{i}"))
                        {
                            OrganizerItemConfig currentItemConfig = itemConfig;
                            GameActions.Print(ImGuiTranslations.Get("Select [DESTINATION] Container for this item"), 82);
                            World.Instance.TargetManager.SetTargeting((destination) =>
                            {
                                if (destination == null || !(destination is Entity destEntity) || !SerialHelper.IsItem(destEntity))
                                {
                                    GameActions.Print(ImGuiTranslations.Get("Only items can be selected!"));
                                    return;
                                }
                                currentItemConfig.DestContSerial = destEntity.Serial;
                                GameActions.Print(ImGuiTranslations.Get("Per-item destination set to ") + $"{destEntity.Serial:X}", 63);
                            });
                        }
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.SetTooltip(ImGuiTranslations.Get("Set per-item destination"));
                        }
                    }

                    ImGui.TableSetColumnIndex(4);
                    bool enabled = itemConfig.Enabled;
                    if (ImGui.Checkbox($"##Enabled{i}", ref enabled))
                        itemConfig.Enabled = enabled;

                    ImGui.TableSetColumnIndex(5);
                    ImGui.PushStyleColor(ImGuiCol.Button, new System.Numerics.Vector4(0.8f, 0.2f, 0.2f, 1.0f));
                    if (ImGui.Button($"X##Delete{i}"))
                    {
                        _selectedConfig.DeleteItemConfig(itemConfig);
                    }

                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip(ImGuiTranslations.Get("Delete this item"));
                    }

                    ImGui.PopStyleColor();
                }

                ImGui.EndTable();
            }
        }
    }
}

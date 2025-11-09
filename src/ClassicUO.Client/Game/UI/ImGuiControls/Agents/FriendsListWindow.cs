using ImGuiNET;
using ClassicUO.Game.Managers;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.UI.Gumps;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;

namespace ClassicUO.Game.UI.ImGuiControls
{
    public class FriendsListWindow : SingletonImGuiWindow<FriendsListWindow>
    {
        private List<FriendEntry> friendEntries;
        private bool showAddFriendPopup = false;
        private string newFriendName = "";

        private FriendsListWindow() : base(ImGuiTranslations.Get("Friends List"))
        {
            WindowFlags = ImGuiWindowFlags.AlwaysAutoResize;
            RefreshFriendsList();
        }

        private void RefreshFriendsList() => friendEntries = FriendsListManager.Instance.GetFriends();

        public override void DrawContent()
        {
            ImGui.Spacing();

            // Header information
            ImGui.Text(ImGuiTranslations.Get("Manage your friends list."));
            ImGui.Spacing();

            // Add friend buttons
            if (ImGui.Button(ImGuiTranslations.Get("Add by Target") + "##FriendsAddTarget"))
            {
                GameActions.Print(World.Instance, ImGuiTranslations.Get("Target a player to add to friends list"));
                World.Instance.TargetManager.SetTargeting(targeted =>
                {
                    if (targeted != null && targeted is Mobile mobile)
                    {
                        if (FriendsListManager.Instance.AddFriend(mobile))
                        {
                            GameActions.Print(World.Instance, string.Format(ImGuiTranslations.Get("Added {0} to friends list"), mobile.Name));
                            RefreshFriendsList();
                        }
                        else
                        {
                            GameActions.Print(World.Instance, string.Format(ImGuiTranslations.Get("Could not add {0} - already in friends list"), mobile.Name));
                        }
                    }
                    else
                    {
                        GameActions.Print(World.Instance, ImGuiTranslations.Get("Invalid target - must be a player"));
                    }
                });
            }

            ImGui.SameLine();

            if (ImGui.Button(ImGuiTranslations.Get("Add by Name") + "##FriendsAddName"))
            {
                showAddFriendPopup = true;
            }

            ImGui.Spacing();
            ImGui.SeparatorText(ImGuiTranslations.Get("Current Friends:"));

            // Display friends list
            if (friendEntries.Count == 0)
            {
                ImGui.Text(ImGuiTranslations.Get("No friends added yet."));
            }
            else
            {
                // Table for friends list
                if (ImGui.BeginTable("FriendsTable", 4, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY, new Vector2(0, ImGuiTheme.Dimensions.STANDARD_TABLE_SCROLL_HEIGHT)))
                {
                    ImGui.TableSetupColumn(ImGuiTranslations.Get("Name"), ImGuiTableColumnFlags.WidthStretch);
                    ImGui.TableSetupColumn(ImGuiTranslations.Get("Serial"), ImGuiTableColumnFlags.WidthFixed, 100);
                    ImGui.TableSetupColumn(ImGuiTranslations.Get("Date Added"), ImGuiTableColumnFlags.WidthFixed, 120);
                    ImGui.TableSetupColumn(ImGuiTranslations.Get("Actions"), ImGuiTableColumnFlags.WidthFixed, 80);
                    ImGui.TableHeadersRow();

                    for (int i = friendEntries.Count - 1; i >= 0; i--)
                    {
                        FriendEntry friend = friendEntries[i];
                        ImGui.TableNextRow();

                        ImGui.TableNextColumn();
                        ImGui.Text(friend.Name ?? ImGuiTranslations.Get("Unknown"));

                        ImGui.TableNextColumn();
                        if (friend.Serial != 0)
                        {
                            ImGui.Text(friend.Serial.ToString());
                        }
                        else
                        {
                            ImGui.TextColored(new Vector4(0.5f, 0.5f, 0.5f, 1.0f), "N/A");
                        }

                        ImGui.TableNextColumn();
                        ImGui.Text(friend.DateAdded.ToString("yyyy-MM-dd"));

                        ImGui.TableNextColumn();
                        if (ImGui.Button(ImGuiTranslations.Get("Remove") + $"##{i}"))
                        {
                            bool removed = friend.Serial != 0
                                ? FriendsListManager.Instance.RemoveFriend(friend.Serial)
                                : FriendsListManager.Instance.RemoveFriend(friend.Name);

                            if (removed)
                            {
                                GameActions.Print(World.Instance, string.Format(ImGuiTranslations.Get("Removed {0} from friends list"), friend.Name));
                                RefreshFriendsList();
                            }
                        }
                    }

                    ImGui.EndTable();
                }
            }

            // Add friend by name popup
            if (showAddFriendPopup)
            {
                ImGui.OpenPopup(ImGuiTranslations.Get("Add Friend by Name") + "##AddFriendPopup");
                showAddFriendPopup = false;
            }

            if (ImGui.BeginPopupModal(ImGuiTranslations.Get("Add Friend by Name") + "##AddFriendPopup"))
            {
                ImGui.Text(ImGuiTranslations.Get("Enter friend's name:"));
                ImGui.SetNextItemWidth(200);
                ImGui.InputText("##FriendName", ref newFriendName, 100);

                ImGui.Spacing();

                if (ImGui.Button(ImGuiTranslations.Get("Add") + "##FriendAddBtn"))
                {
                    if (!string.IsNullOrWhiteSpace(newFriendName))
                    {
                        if (FriendsListManager.Instance.AddFriend(0, newFriendName.Trim()))
                        {
                            GameActions.Print(World.Instance, string.Format(ImGuiTranslations.Get("Added {0} to friends list"), newFriendName.Trim()));
                            RefreshFriendsList();
                            newFriendName = "";
                            ImGui.CloseCurrentPopup();
                        }
                        else
                        {
                            GameActions.Print(World.Instance, string.Format(ImGuiTranslations.Get("Could not add {0} - already in friends list or invalid name"), newFriendName.Trim()));
                        }
                    }
                }

                ImGui.SameLine();

                if (ImGui.Button(ImGuiTranslations.Get("Cancel") + "##FriendCancelBtn"))
                {
                    newFriendName = "";
                    ImGui.CloseCurrentPopup();
                }

                ImGui.EndPopup();
            }
        }
    }
}
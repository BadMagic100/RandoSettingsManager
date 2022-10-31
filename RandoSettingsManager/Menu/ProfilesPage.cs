using MenuChanger;
using MenuChanger.Extensions;
using MenuChanger.MenuElements;
using MenuChanger.MenuPanels;
using RandoSettingsManager.SettingsManagement;
using RandoSettingsManager.SettingsManagement.Filer.Disk;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace RandoSettingsManager.Menu
{
    internal class ProfilesPage
    {
        private static readonly MethodInfo setEntryFieldValueUnvalidated = typeof(EntryField<string>)
            .GetProperty(nameof(EntryField<string>.Value), typeof(string))
            .GetSetMethod(true);

        private static readonly MethodInfo setEntryFieldValidatedInputUnvalidated = typeof(EntryField<string>)
            .GetProperty(nameof(EntryField<string>.ValidatedInput), typeof(string))
            .GetSetMethod(true);

        private static void SetEntryFieldValueUnvalidated(EntryField<string> field, string value)
        {
            object[] args = new object[] { value };
            setEntryFieldValueUnvalidated.Invoke(field, args);
            setEntryFieldValidatedInputUnvalidated.Invoke(field, new object[] { value });
            field.InputField.text = value;
        }

        public MenuPage RootPage { get; }

        private readonly MultiGridItemPanel profilePanel;
        private readonly SmallButton newProfile;

        private readonly MenuPage createPage;
        private readonly TextEntryField createNameEntry;
        private readonly MenuLabel createProfileStatus;

        private readonly MenuPage editPage;
        private readonly TextEntryField editNameEntry;
        private readonly MenuLabel editProfileStatus;

        private string? selectedProfile;

        public ProfilesPage(MenuPage parent)
        {
            RootPage = new("RandoSettingsManager Manage Profiles Page", parent);
            editPage = new("RandoSettingsManager Edit Profile Page", RootPage);
            createPage = new("RandoSettingsManager Create Profile Page", RootPage);

            // root page

            RootPage.AfterShow += () =>
            {
                LockMenu();
                new Thread(DoReloadProfiles).Start();
            };

            MenuLabel title = new(RootPage, "Manage Profiles", MenuLabel.Style.Title);
            title.MoveTo(SpaceParameters.TOP_CENTER);
            newProfile = new(RootPage, "Create New");
            newProfile.AddHideAndShowEvent(createPage);
            profilePanel = new(RootPage, 5, 3, 60f, 650f, SpaceParameters.TOP_CENTER_UNDER_TITLE, newProfile);

            // create page

            createNameEntry = new(createPage, "Profile Name");
            createNameEntry.InputField.characterLimit = 30;
            createNameEntry.InputField.lineType = InputField.LineType.SingleLine;
            createNameEntry.InputField.textComponent.rectTransform.sizeDelta = new Vector2(700, 800);
            createNameEntry.ModifyInputString += MakeLegalFilename;

            SmallButton save = new(createPage, "Create Profile");
            save.OnClick += CreateProfile;
            createProfileStatus = new MenuLabel(createPage, "", MenuLabel.Style.Body);
            createProfileStatus.Text.alignment = TextAnchor.UpperCenter;

            VerticalItemPanel createVip = new(createPage, SpaceParameters.TOP_CENTER_UNDER_TITLE, 
                SpaceParameters.VSPACE_MEDIUM, true,
                createNameEntry, save, createProfileStatus);
            createPage.AfterHide += () =>
            {
                SetEntryFieldValueUnvalidated(createNameEntry, "");
                createProfileStatus.Text.text = "";
            };

            // edit page

            editNameEntry = new(editPage, "Profile Name");
            editNameEntry.InputField.characterLimit = 30;
            editNameEntry.InputField.lineType = InputField.LineType.SingleLine;
            editNameEntry.InputField.textComponent.rectTransform.sizeDelta = new Vector2(700, 800);
            editNameEntry.ModifyInputString += MakeLegalFilename;

            SmallButton load = new(editPage, "Load Profile");
            load.OnClick += LoadSettings;
            SmallButton overwrite = new(editPage, "Overwrite Settings");
            overwrite.OnClick += OverwriteSettings;
            SmallButton delete = new(editPage, "Delete Profile");
            delete.OnClick += DeleteSettings;
            editProfileStatus = new MenuLabel(editPage, "", MenuLabel.Style.Body);
            editProfileStatus.Text.alignment = TextAnchor.UpperCenter;

            VerticalItemPanel editVip = new(editPage, SpaceParameters.TOP_CENTER_UNDER_TITLE, 
                SpaceParameters.VSPACE_MEDIUM, true,
                editNameEntry, load, overwrite, delete, editProfileStatus);
            editPage.AfterHide += RenameProfile;
        }

        private void DoReloadProfiles()
        {
            try
            {
                string[] dirs = Directory.GetDirectories(SettingsMenu.ProfilesDir);
                ThreadSupport.BlockUntilInvoked(() =>
                {
                    for (int i = 1; i < profilePanel.Items.Count; i++)
                    {
                        profilePanel.Items[i].Destroy();
                    }
                    profilePanel.Clear();
                    profilePanel.Add(newProfile);
                    foreach (string dir in dirs)
                    {
                        string name = Path.GetFileName(dir);
                        SmallButton prof = new(RootPage, name);
                        prof.OnClick += () =>
                        {
                            selectedProfile = name;
                            SetEntryFieldValueUnvalidated(editNameEntry, name);
                        };
                        prof.AddHideAndShowEvent(editPage);
                        profilePanel.Add(prof);
                    }
                });
            }
            catch (Exception ex)
            {
                RandoSettingsManagerMod.Instance.LogError(ex);
            }
            finally
            {
                ThreadSupport.BeginInvoke(UnlockMenu);
            }
        }

        private void MakeLegalFilename(ref string newValue, string orig)
        {
            HashSet<char> invalid = new(Path.GetInvalidFileNameChars());
            StringBuilder b = new();
            foreach (char c in newValue)
            {
                if (!invalid.Contains(c))
                {
                    b.Append(c);
                }
            }
            newValue = b.ToString().Trim();
            string newPath = Path.Combine(SettingsMenu.ProfilesDir, newValue);
            if (string.IsNullOrWhiteSpace(newValue))
            {
                newValue = orig;
            }
            if (newValue != orig && newValue != selectedProfile && Directory.Exists(newPath))
            {
                newValue = orig;
            }
        }

        private void CreateProfile()
        {
            string path = Path.Combine(SettingsMenu.ProfilesDir, createNameEntry.Value);
            if (string.IsNullOrWhiteSpace(createNameEntry.Value))
            {
                createProfileStatus.Text.text = "Please enter a valid profile name";
                return;
            }
            if (Directory.Exists(path))
            {
                createProfileStatus.Text.text = "Please enter a unique profile name";
                return;
            }

            SettingsManager manager = RandoSettingsManagerMod.Instance.settingsManager;

            try
            {

                manager.SaveSettings(new DiskFiler(path).RootDirectory, false, false);
                createPage.Hide();
                createPage.backTo.Show();
            }
            catch (Exception ex)
            {
                RandoSettingsManagerMod.Instance.LogError(ex);
                createProfileStatus.Text.text = "Unexpected error saving settings";
            }
        }

        private void LoadSettings()
        {
            SettingsManager manager = RandoSettingsManagerMod.Instance.settingsManager;

            string dir = Path.Combine(SettingsMenu.ProfilesDir, selectedProfile);
            if (!Directory.Exists(dir))
            {
                editProfileStatus.Text.text = "Selected profile folder was deleted or renamed";
            }

            try
            {
                manager.LoadSettings(new DiskFiler(dir).RootDirectory, false);
                editProfileStatus.Text.text = "Loaded successfully!";
            }
            catch (ValidationException ve)
            {
                editProfileStatus.Text.text = ve.Message;
            }
            catch (Exception ex)
            {
                RandoSettingsManagerMod.Instance.LogError(ex);
                editProfileStatus.Text.text = "Unexpected error loading settings";
            }
        }

        private void OverwriteSettings()
        {
            SettingsManager manager = RandoSettingsManagerMod.Instance.settingsManager;

            try
            {
                string path = Path.Combine(SettingsMenu.ProfilesDir, selectedProfile);
                manager.SaveSettings(new DiskFiler(path).RootDirectory, false, false);
                editProfileStatus.Text.text = "Saved successfully!";
            }
            catch (Exception ex)
            {
                RandoSettingsManagerMod.Instance.LogError(ex);
                editProfileStatus.Text.text = "Unexpected error saving settings";
            }
        }

        private void DeleteSettings()
        {
            try
            {
                string path = Path.Combine(SettingsMenu.ProfilesDir, selectedProfile);
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
                editPage.Hide();
                editPage.backTo.Show();
            }
            catch (Exception ex)
            {
                RandoSettingsManagerMod.Instance.LogWarn($"Failed to delete profile: {ex}");
            }
        }

        private void RenameProfile()
        {
            try
            {
                string path = Path.Combine(SettingsMenu.ProfilesDir, selectedProfile);
                if (Directory.Exists(path))
                {
                    string newPath = Path.Combine(SettingsMenu.ProfilesDir, editNameEntry.Value);
                    if (path != newPath)
                    {
                        Directory.Move(path, newPath);
                    }
                }
            }
            catch (Exception ex)
            {
                RandoSettingsManagerMod.Instance.LogWarn(ex);
            }

            // clean up
            selectedProfile = null;
            editProfileStatus.Text.text = "";
        }

        private void LockMenu()
        {
            RootPage.backButton.Lock();
            foreach (SmallButton b in profilePanel.Items.OfType<SmallButton>())
            {
                b.Lock();
            }
        }

        private void UnlockMenu()
        {
            RootPage.backButton.Unlock();
            foreach (SmallButton b in profilePanel.Items.OfType<SmallButton>())
            {
                b.Unlock();
            }
        }
    }
}

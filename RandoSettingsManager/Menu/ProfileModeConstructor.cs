using MenuChanger;
using MenuChanger.MenuElements;
using Modding;
using RandomizerMod.Menu;
using RandoSettingsManager.SettingsManagement;
using RandoSettingsManager.SettingsManagement.Filer.Disk;
using System;
using System.IO;
using UnityEngine.UI;

namespace RandoSettingsManager.Menu
{
    internal class ProfileModeConstructor : ModeMenuConstructor
    {
        private readonly string profileName;
        private readonly string dirName;

        private bool lockedOut = false;

        public ProfileModeConstructor(string profileName)
        {
            this.profileName = profileName;
            this.dirName = Path.Combine(SettingsMenu.ProfilesDir, profileName);
        }

        public override void OnEnterMainMenu(MenuPage modeMenu) { }

        public override void OnExitMainMenu() { }

        public override bool TryGetModeButton(MenuPage modeMenu, out BigButton button)
        {
            if (!Directory.Exists(dirName))
            {
                RandoSettingsManagerMod.Instance.GS.ModeProfiles.Remove(profileName);
                button = null!;
                return false;
            }

            MenuPage randoHomePage = ReflectionHelper.GetField<RandomizerMenu, MenuPage>(
                RandomizerMenuAPI.Menu, "StartPage");
            BigButton navButton = new(modeMenu, 
                RandomizerMod.RandomizerMod.SpriteManager.GetSprite("logo"),
                profileName, "Randomizer Profile");

            navButton.OnClick += () =>
            {
                if (lockedOut)
                {
                    return;
                }

                if (TryApplyProfile())
                {
                    modeMenu.Hide();
                    randoHomePage.Show();
                }
                else
                {
                    lockedOut = true;
                    SetVisualButtonState(navButton);
                }
            };
            SetVisualButtonState(navButton);
            button = navButton;
            return true;
        }

        private void SetVisualButtonState(BigButton button)
        {
            if (lockedOut)
            {
                button.Button.enabled = false;
                button.GameObject.transform.Find("Text").GetComponent<Text>().color = Colors.FALSE_COLOR;
                button.GameObject.transform.Find("Image").GetComponent<Image>().color = Colors.FALSE_COLOR;
                Text desc = button.GameObject.transform.Find("DescriptionText").GetComponent<Text>();
                desc.text = "Invalid Profile, See Mod Log";
                desc.color = Colors.FALSE_COLOR;
            }
            else
            {
                button.Button.enabled = true;
                button.GameObject.transform.Find("Text").GetComponent<Text>().color = Colors.DEFAULT_COLOR;
                button.GameObject.transform.Find("Image").GetComponent<Image>().color = Colors.DEFAULT_COLOR;
                Text desc = button.GameObject.transform.Find("DescriptionText").GetComponent<Text>();
                desc.text = "Randomizer Profile";
                desc.color = Colors.DEFAULT_COLOR;
            }
        }

        private bool TryApplyProfile()
        {
            SettingsManager manager = RandoSettingsManagerMod.Instance.settingsManager;
            if (!Directory.Exists(dirName))
            {
                RandoSettingsManagerMod.Instance.LogError("Profile not found: " + profileName);
                return false;
            }

            try
            {
                manager.LoadSettings(new DiskFiler(dirName).RootDirectory, false);
                return true;
            }
            catch (Exception ex)
            {
                RandoSettingsManagerMod.Instance.LogError(ex);
                return false;
            }
        }
    }
}

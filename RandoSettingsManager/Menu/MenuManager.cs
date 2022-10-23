using MenuChanger;
using MenuChanger.MenuElements;
using MenuChanger.MenuPanels;
using Modding;
using RandomizerMod.Menu;
using System;
using UnityEngine;

namespace RandoSettingsManager.Menu
{
    internal class MenuManager
    {
        public static void HookMenu()
        {
            RandomizerMenuAPI.AddMenuPage(Construct, NoOpConstructButton!);
        }

        private static void Construct(MenuPage connectionPage)
        {
            RandomizerMenu rm = RandomizerMenuAPI.Menu;
            SmallButton manageBtn = ReflectionHelper.GetField<RandomizerMenu, SmallButton>(
                rm, "ToManageSettingsPageButton");

            MenuPage classic = ReflectionHelper.GetField<RandomizerMenu, MenuPage>(
                rm, "ManageSettingsPage");

            MenuPage modern = new("RandoSettingsManager Manage Settings", manageBtn.Parent);

            PatchRandoMenuPages(manageBtn, classic, modern);
            BuildManagePage(classic, modern);
        }

        private static void BuildManagePage(MenuPage classic, MenuPage modern)
        {
            SmallButton navToClassic = new(modern, "Classic Settings Management");
            navToClassic.MoveTo(new(0, -450 + SpaceParameters.VSPACE_SMALL));
            modern.ReplaceNavigation(new HorizontalNavWithItemAboveBackButton(modern, navToClassic));

            navToClassic.OnClick += () =>
            {
                RandoSettingsManagerMod.Instance.GS.Mode = SettingsManagementMode.Classic;
                VisitSettingsPageForCurrentMode(navToClassic, classic, modern);
            };

            new VerticalItemPanel(modern, new Vector2(-400, 300), SpaceParameters.VSPACE_SMALL, true,
                new SmallButton(modern, "hello"),
                new SmallButton(modern, "world"));

            new VerticalItemPanel(modern, new Vector2(400, 300), SpaceParameters.VSPACE_SMALL, true,
               new SmallButton(modern, "hello"),
               new SmallButton(modern, "world"));
        }

        private static void PatchRandoMenuPages(SmallButton manageButton,
            MenuPage classic, MenuPage modern)
        {
            ReflectionHelper.SetField<BaseButton, Action?>(manageButton, nameof(SmallButton.OnClick), null);

            SmallButton navToModern = new(classic, "Modern Settings Management");
            navToModern.MoveTo(new(0, -450 + SpaceParameters.VSPACE_SMALL));
            classic.ReplaceNavigation(new HorizontalNavWithItemAboveBackButton(classic, navToModern));

            manageButton.OnClick += () => VisitSettingsPageForCurrentMode(manageButton, classic, modern);
            navToModern.OnClick += () =>
            {
                RandoSettingsManagerMod.Instance.GS.Mode = SettingsManagementMode.Modern;
                VisitSettingsPageForCurrentMode(navToModern, classic, modern);
            };
        }

        private static void VisitSettingsPageForCurrentMode(SmallButton sender, MenuPage classic, MenuPage modern)
        {
            sender.Parent.Hide();
            switch (RandoSettingsManagerMod.Instance.GS.Mode)
            {
                case SettingsManagementMode.Modern:
                    modern.Show();
                    break;
                case SettingsManagementMode.Classic:
                    classic.Show();
                    break;
                default:
                    throw new NotImplementedException($"Missing mode handler for {RandoSettingsManagerMod.Instance.GS.Mode}");

            }
        }

        private static bool NoOpConstructButton(MenuPage connectionPage, out SmallButton? b)
        {
            b = null;
            return false;
        }
    }
}

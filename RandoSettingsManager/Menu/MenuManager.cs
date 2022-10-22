using MenuChanger;
using MenuChanger.Extensions;
using MenuChanger.MenuElements;
using MenuChanger.MenuPanels;
using Modding;
using RandomizerMod.Menu;
using System;

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
            VerticalItemPanel codeVip = ReflectionHelper.GetField<RandomizerMenu, VerticalItemPanel>(
                rm, "CodeVIP");
            VerticalItemPanel profileVip = ReflectionHelper.GetField<RandomizerMenu, VerticalItemPanel>(
                rm, "ProfileVIP");

            MenuPage modern = new("RandoSettingsManager Manage Settings", manageBtn.Parent);

            PatchRandoMenuPages(manageBtn, codeVip, profileVip, classic, modern);
            BuildManagePage(classic, modern);
        }

        private static void BuildManagePage(MenuPage classic, MenuPage modern)
        {
            SmallButton navToClassic = new(modern, "Classic Settings Management");
            //modern.nav = new HorizontalNavWithItemAboveBackButton(modern, navToClassic);

            navToClassic.MoveTo(new(0, -450 + SpaceParameters.VSPACE_SMALL));
            navToClassic.SymSetNeighbor(Neighbor.Down, modern.backButton);
            // todo - set upward nav with some custom arrangement doodad

            navToClassic.OnClick += () =>
            {
                RandoSettingsManagerMod.Instance.GS.Mode = SettingsManagementMode.Classic;
                VisitSettingsPageForCurrentMode(navToClassic, classic, modern);
            };
        }

        private static void PatchRandoMenuPages(SmallButton manageButton, 
            VerticalItemPanel codeVip, VerticalItemPanel profileVip,
            MenuPage classic, MenuPage modern)
        {
            ReflectionHelper.SetField<BaseButton, Action?>(manageButton, nameof(SmallButton.OnClick), null);

            SmallButton navToModern = new(classic, "Modern Settings Management");
            navToModern.MoveTo(new(0, -450 + SpaceParameters.VSPACE_SMALL));
            navToModern.SymSetNeighbor(Neighbor.Down, classic.backButton);

            navToModern.SetNeighbor(Neighbor.Up, codeVip);
            codeVip.SetNeighbor(Neighbor.Down, navToModern);
            profileVip.SetNeighbor(Neighbor.Down, navToModern);

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

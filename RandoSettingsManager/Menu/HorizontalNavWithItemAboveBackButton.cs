using MenuChanger;
using MenuChanger.Extensions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

namespace RandoSettingsManager.Menu
{
    /// <summary>
    /// Menu nav strategy which allows the placement of an element directly above the back button.
    /// Assumes that a back button actually exists on the page.
    /// </summary>
    internal class HorizontalNavWithItemAboveBackButton : MenuPageNavigation
    {
        readonly ISelectable aboveBackButton;
        readonly List<ISelectable> Selectables = new();

        public HorizontalNavWithItemAboveBackButton(MenuPage page, ISelectable aboveBackButton) : base(page)
        {
            this.aboveBackButton = aboveBackButton;
            SetNavWhileEmpty();
        }

        public override void Add(ISelectable selectable)
        {
            if (Selectables.Count > 0)
            {
                SetItemNav(selectable, false);
                selectable.SymSetNeighbor(Neighbor.Right, Selectables.First());
                selectable.SymSetNeighbor(Neighbor.Left, Selectables.Last());
            }
            else
            {
                SetItemNav(selectable, true);
                selectable.SetNeighbor(Neighbor.Left, selectable);
                selectable.SetNeighbor(Neighbor.Right, selectable);
            }

            Selectables.Add(selectable);
        }

        public override void Remove(ISelectable selectable)
        {
            int i = Selectables.IndexOf(selectable);
            if (i >= 0)
            {
                Selectables.RemoveAt(i);
                if (Selectables.Count > 0)
                {
                    ISelectable left = Selectables[IndexWithWraparound(i - 1)];
                    ISelectable right = Selectables[IndexWithWraparound(i)];
                    left.SymSetNeighbor(Neighbor.Right, right);

                    if (i == 0)
                    {
                        SetItemNav(right, true);
                    }
                }
                else
                {
                    SetNavWhileEmpty();
                }
            }
        }

        public override void ResetNavigation()
        {
            if (Selectables.Count == 0)
            {
                SetNavWhileEmpty();
            }
            else
            {
                foreach (ISelectable sel in Selectables)
                {
                    if (sel is ISelectableGroup isg)
                    {
                        isg.ResetNavigation();
                    }
                }

                for (int i = 0; i < Selectables.Count; i++)
                {
                    int left = IndexWithWraparound(i - 1);
                    Selectables[i].SymSetNeighbor(Neighbor.Left, Selectables[left]);
                    Selectables[i].SetNeighbor(Neighbor.Up, Page.backButton);
                    Selectables[i].SetNeighbor(Neighbor.Down, aboveBackButton);
                }
            }
        }

        public override void SelectDefault()
        {
            if (Selectables.Count > 0)
            {
                Selectable s1 = Selectables[0].GetSelectable(Neighbor.Up);
                if (s1)
                {
                    s1.Select();
                    return;
                }
            }

            Selectable s2 = aboveBackButton.GetSelectable(Neighbor.Up);
            if (s2)
            {
                s2.Select();
                return;
            }

            Page.backButton.Button.Select();
        }

        /// <summary>
        /// Wraps an integer index into a value that is within the bounds of the selectable collection. Only
        /// well-defined when collection is non-empty.
        /// </summary>
        private int IndexWithWraparound(int i)
        {
            int j = i % Selectables.Count;
            if (j < 0)
            {
                j += Selectables.Count;
            }
            return j;
        }

        private void SetNavWhileEmpty()
        {
            aboveBackButton.SetNeighbor(Neighbor.Up, Page.backButton);
            Page.backButton.SetNeighbor(Neighbor.Down, aboveBackButton);
        }

        private void SetItemNav(ISelectable selectable, bool first)
        {
            selectable.SetNeighbor(Neighbor.Up, Page.backButton);
            selectable.SetNeighbor(Neighbor.Down, aboveBackButton);

            if (first)
            {
                Page.backButton.SetNeighbor(Neighbor.Down, selectable);
                aboveBackButton.SetNeighbor(Neighbor.Up, selectable);
            }
        }
    }
}

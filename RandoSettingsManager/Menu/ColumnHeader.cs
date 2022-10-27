using MenuChanger;
using MenuChanger.MenuElements;

namespace RandoSettingsManager.Menu
{
    internal class ColumnHeader : MenuLabel
    {
        public ColumnHeader(MenuPage page, string text) : base(page, text, Style.Body)
        {
            Text.alignment = UnityEngine.TextAnchor.UpperCenter;
        }
    }
}

using MenuChanger;
using MenuChanger.MenuElements;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UObject = UnityEngine.Object;

namespace RandoSettingsManager.Menu
{
    internal class Messager : MenuLabel
    {
        private const string ellipses = "...";
        private bool full = false;

        public Messager(MenuPage page) : base(page, "", Style.Body)
        {
            UObject.Destroy(GameObject.GetComponent<ContentSizeFitter>());
            // apparently if we wanted to be narrower we'd set the X negative because why wouldn't we
            GameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 700);
        }

        public void Clear()
        {
            Text.text = "";
            full = false;
        }

        public void Write(string str)
        {
            if (full)
            {
                return;
            }
            Text.text += str;
            Truncate();
        }

        public void WriteLine(string str)
        {
            if (full)
            {
                RandoSettingsManagerMod.Instance.LogDebug("Cannot add more text, messager is full");
                return;
            }
            Text.text += str + "\n";
            Truncate();
        }

        private void Truncate()
        {
            TextGenerator gen = new();
            TextGenerationSettings settings = Text.GetGenerationSettings(Text.rectTransform.rect.size);
            gen.Populate(Text.text, settings);

            int diff = Text.text.Length - gen.characterCountVisible;
            if (diff > 0)
            {
                full = true;

                int start = gen.lines.Last().startCharIdx;
                int end = gen.characterCountVisible - 1;
                int len = end - start + 1;
                string lastLine = Text.text.Substring(start, len);
                float lw = gen.GetPreferredWidth(lastLine, settings);
                float ew = gen.GetPreferredWidth(ellipses, settings);

                Text.text = Text.text.Remove(end + 1);

                if (ew + lw <= Text.rectTransform.rect.width)
                {
                    Text.text += ellipses;
                }
                else
                {
                    Text.text = Text.text.Remove(end - 2) + ellipses;
                }
            }
        }
    }
}

using UnityEngine;

namespace DOTSDemo.Shared
{
    public static class GUIStyleHelper
    {
        public static Texture2D MakeTexture(int width, int height, Color color)
        {
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }

            Texture2D texture = new Texture2D(width, height);
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

        public static GUIStyle DarkPanel()
        {
            return new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.UpperLeft,
                padding = new RectOffset(12, 12, 10, 10),
                normal = { background = MakeTexture(2, 2, new Color(0.08f, 0.08f, 0.1f, 0.86f)) }
            };
        }

        public static GUIStyle LightPanel()
        {
            return new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.UpperLeft,
                padding = new RectOffset(12, 12, 10, 10)
            };
        }

        public static GUIStyle TitleLabel(int fontSize = 20)
        {
            return new GUIStyle(GUI.skin.label)
            {
                fontSize = fontSize,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperLeft,
                wordWrap = false,
                normal = { textColor = Color.white }
            };
        }

        public static GUIStyle BodyLabel(int fontSize = 13)
        {
            return new GUIStyle(GUI.skin.label)
            {
                fontSize = fontSize,
                alignment = TextAnchor.UpperLeft,
                wordWrap = true,
                normal = { textColor = new Color(0.78f, 0.8f, 0.86f) }
            };
        }

        public static GUIStyle HintLabel(int fontSize = 13)
        {
            return new GUIStyle(GUI.skin.label)
            {
                fontSize = fontSize,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = false,
                normal = { textColor = new Color(0.7f, 0.72f, 0.78f) }
            };
        }
    }
}

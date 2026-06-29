using UnityEngine;
using UnityEngine.UI;

namespace Lab.UI
{
    public static class HydrodynamicsUiTheme
    {
        public static readonly Color Backdrop = new Color(0.03f, 0.05f, 0.07f, 0.68f);
        public static readonly Color Panel = new Color(0.80f, 0.89f, 0.91f, 0.98f);
        public static readonly Color PanelDeep = new Color(0.18f, 0.25f, 0.29f, 0.96f);
        public static readonly Color Surface = new Color(0.93f, 0.98f, 0.99f, 0.96f);
        public static readonly Color SurfaceMuted = new Color(0.70f, 0.82f, 0.86f, 0.92f);
        public static readonly Color Field = new Color(0.96f, 0.99f, 1.00f, 1f);
        public static readonly Color FieldDisabled = new Color(0.72f, 0.82f, 0.86f, 0.68f);
        public static readonly Color Water = new Color(0.13f, 0.56f, 0.76f, 1f);
        public static readonly Color WaterLight = new Color(0.35f, 0.78f, 0.93f, 1f);
        public static readonly Color WaterDark = new Color(0.06f, 0.33f, 0.48f, 1f);
        public static readonly Color Accent = new Color(0.95f, 0.45f, 0.18f, 1f);
        public static readonly Color Text = new Color(0.05f, 0.13f, 0.17f, 1f);
        public static readonly Color TextOnDark = new Color(0.92f, 0.98f, 1f, 1f);
        public static readonly Color MutedText = new Color(0.29f, 0.43f, 0.49f, 1f);

        public static ColorBlock ButtonColors(Color baseColor)
        {
            ColorBlock colors = ColorBlock.defaultColorBlock;
            colors.normalColor = baseColor;
            colors.highlightedColor = WaterLight;
            colors.pressedColor = WaterDark;
            colors.selectedColor = WaterLight;
            colors.disabledColor = new Color(0.48f, 0.57f, 0.60f, 0.55f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.08f;
            return colors;
        }
    }
}

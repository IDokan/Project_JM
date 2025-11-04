using UnityEngine;

namespace GemEnums
{
    public enum GemColor
    {
        None,
        Red,
        Green,
        Blue,
        Yellow,
    }

    public static class GemColorUtility
    {
        public static Color ConvertGemColor(this GemColor color)
        {
            switch (color)
            {
                case GemColor.Red: return Color.red;
                case GemColor.Green: return Color.green;
                case GemColor.Blue: return Color.blue;
                case GemColor.Yellow: return Color.yellow;
                default: return Color.white;
            }
        }
    }

}
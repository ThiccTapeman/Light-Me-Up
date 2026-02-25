using System;
using UnityEngine;

namespace ThiccTapeman.Utils
{
    public static class ColorUtils
    {
        public static Color GenerateRandomColor()
        {
            float r = UnityEngine.Random.Range(0, 255) / 255.0f;
            float g = UnityEngine.Random.Range(0, 255) / 255.0f;
            float b = UnityEngine.Random.Range(0, 255) / 255.0f;

            return new Color(r, g, b);
        }

        public static string ColorToHex(Color color, bool includeTag = true)
        {
            return (includeTag?"#":"") + string.Format(string.Format("{0:x}", (int)color.r)) + string.Format(string.Format("{0:x}", (int)color.g)) + string.Format(string.Format("{0:x}", (int)color.b));
        }

        public static Color HexToColor(string hex)
        {
            hex.TrimStart('#');
            
            if(hex.Length == 6 || hex.Length == 3) 
            {
                int r;
                int g;
                int b;

                if(hex.Length == 3)
                {
                    r = Convert.ToInt32("" + hex[0] + hex[0], 16);
                    g = Convert.ToInt32("" + hex[1] + hex[1], 16);
                    b = Convert.ToInt32("" + hex[2] + hex[2], 16);

                    return new Color(r, g, b);
                }

                r = Convert.ToInt32("" + hex[0] + hex[1], 16);
                g = Convert.ToInt32("" + hex[2] + hex[3], 16);
                b = Convert.ToInt32("" + hex[4] + hex[5], 16);

                return new Color(r, g, b);
            }

            return Color.black;
        }

        public static Color AddColors(Color a, Color b)
        {
            return new Color(a.r + b.r, a.g + b.g, a.b + b.b);
        }
        public static Color AddColors(Color a, float b)
        {
            return new Color(a.r + b, a.g + b, a.b + b);
        }

        public static Color SubtractColors(Color a, Color b)
        {
            return new Color(a.r - b.r, a.g - b.g, a.b - b.b);
        }
        public static Color SubtractColors(Color a, float b)
        {
            return new Color(a.r - b, a.g - b, a.b - b);
        }


        public static Color MultiplyColors(Color a, Color b)
        {
            return new Color(a.r * b.r, a.g * b.g, a.b * b.b);
        }
        public static Color MultiplyColors(Color a, float b)
        {
            return new Color(a.r * b, a.g * b, a.b * b);
        }
    }
}

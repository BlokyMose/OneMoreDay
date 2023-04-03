using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Encore.Utility
{
    public static class ColorUtility
    {

        #region [Named Colors]
        public static Color whiteSmoke { get { return new Color32(245, 245, 245, 255); } }
        public static Color gainsboro { get { return new Color32(220, 220, 220, 255); } }
        public static Color silver { get { return new Color32(192, 192, 192, 255); } }
        public static Color paleGreen { get { return new Color32(152, 251, 152, 255); } }
        public static Color salmon { get { return new Color32(250, 128, 114, 255); } }
        public static Color tomato { get { return new Color32(255, 99, 71, 255); } }
        public static Color fireBrick { get { return new Color32(178, 34, 34, 255); } }
        public static Color orange { get { return new Color32(255, 165, 0, 255); } }
        public static Color goldenRod { get { return new Color32(218, 165, 32, 255); } }




        #endregion



        public static Color TransitionColor(this Color color, Color targetColor, float speed)
        {
            if (color.r - speed > targetColor.r)
                color = color.ChangeRed(color.r - speed);
            else if (color.r + speed < targetColor.r)
                color = color.ChangeRed(color.r + speed);
            else
                color = color.ChangeRed(targetColor.r);

            if (color.g - speed > targetColor.g)
                color = color.ChangeGreen(color.g - speed);
            else if (color.g + speed < targetColor.g)
                color = color.ChangeGreen(color.g + speed);
            else
                color = color.ChangeGreen(targetColor.g);

            if (color.b - speed > targetColor.b)
                color = color.ChangeBlue(color.b - speed);
            else if (color.b + speed < targetColor.b)
                color = color.ChangeBlue(color.b + speed);
            else
                color = color.ChangeBlue(targetColor.b);

            if (color.a - speed > targetColor.a)
                color = color.ChangeAlpha(color.a - speed);
            else if (color.a + speed < targetColor.a)
                color = color.ChangeAlpha(color.a + speed);
            else
                color = color.ChangeAlpha(targetColor.a);

            return color;
        }

        public static Color ChangeAlpha(this Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }

        public static Color ChangeRed(this Color color, float red)
        {
            return new Color(red, color.g, color.b, color.a);
        }

        public static Color ChangeGreen(this Color color, float green)
        {
            return new Color(color.r, green, color.b, color.a);
        }

        public static Color ChangeBlue(this Color color, float blue)
        {
            return new Color(color.r, color.g, blue, color.a);
        }

        public static Color HSLToRGB(float h, float s, float l, float a)
        {
            float r, g, b;

            if (s == 0)
            {
                r = g = b = l; // achromatic
            }
            else
            {
                float HueToRGB(float p, float q, float t)
                {
                    if (t < 0) t += 1;
                    if (t > 1f) t -= 1;
                    if (t < 1f / 6f) return p + (q - p) * 6f * t;
                    if (t < 1f / 2f) return q;
                    if (t < 2f / 3f) return p + (q - p) * (2f / 3f - t) * 6f;
                    return p;
                }

                var q = l < 0.5f ? l * (1f + s) : l + s - l * s;
                var p = 2f * l - q;
                r = HueToRGB(p, q, h + 1f / 3f);
                g = HueToRGB(p, q, h);
                b = HueToRGB(p, q, h - 1f / 3f);
            }

            return new Color(r, g, b, a);
        }

        public static bool ToColor(string text, out Color color)
        {
            text = text.Trim();
            if (string.Equals(text, "red", System.StringComparison.CurrentCultureIgnoreCase))
            {
                color = Color.red;
                return true;
            }
            else if (string.Equals(text, "green", System.StringComparison.CurrentCultureIgnoreCase))
            {
                color = Color.green;
                return true;
            }
            else if (string.Equals(text, "blue", System.StringComparison.CurrentCultureIgnoreCase))
            {
                color = Color.blue;
                return true;
            }
            else if (string.Equals(text, "cyan", System.StringComparison.CurrentCultureIgnoreCase))
            {
                color = Color.cyan;
                return true;
            }
            else if (string.Equals(text, "yellow", System.StringComparison.CurrentCultureIgnoreCase))
            {
                color = Color.yellow;
                return true;
            }
            else if (string.Equals(text, "magenta", System.StringComparison.CurrentCultureIgnoreCase))
            {
                color = Color.magenta;
                return true;
            }
            else if (string.Equals(text, "black", System.StringComparison.CurrentCultureIgnoreCase))
            {
                color = Color.black;
                return true;
            }
            else if (string.Equals(text, "white", System.StringComparison.CurrentCultureIgnoreCase))
            {
                color = Color.white;
                return true;
            }
            else if (string.Equals(text, "gray", System.StringComparison.CurrentCultureIgnoreCase) || string.Equals(text, "grey", System.StringComparison.CurrentCultureIgnoreCase))
            {
                color = Color.gray;
                return true;
            }
            else if (string.Equals(text, "clear", System.StringComparison.CurrentCultureIgnoreCase))
            {
                color = Color.clear;
                return true;
            }
            else
            {
                color = Color.white;
                return false;
            }
        }

    }
}
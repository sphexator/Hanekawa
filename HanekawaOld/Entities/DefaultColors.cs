using System;
using Discord;
using Hanekawa.Entities.Interfaces;

namespace Hanekawa.Entities
{
    public class DefaultColors : IHanaService
    {
        private static readonly Color Teal = new Color(1752220U);
        private static readonly Color DarkTeal = new Color(1146986U);
        private static readonly Color Green = new Color(3066993U);
        private static readonly Color DarkGreen = new Color(2067276U);
        private static readonly Color Blue = new Color(3447003U);
        private static readonly Color DarkBlue = new Color(2123412U);
        private static readonly Color Purple = new Color(10181046U);
        private static readonly Color DarkPurple = new Color(7419530U);
        private static readonly Color Magenta = new Color(15277667U);
        private static readonly Color DarkMagenta = new Color(11342935U);
        private static readonly Color Gold = new Color(15844367U);
        private static readonly Color LightOrange = new Color(12745742U);
        private static readonly Color Orange = new Color(15105570U);
        private static readonly Color DarkOrange = new Color(11027200U);
        private static readonly Color Red = new Color(15158332U);
        private static readonly Color DarkRed = new Color(10038562U);
        private static readonly Color LightGrey = new Color(9936031U);
        private static readonly Color LighterGrey = new Color(9807270U);
        private static readonly Color DarkGrey = new Color(6323595U);
        private static readonly Color DarkerGrey = new Color(5533306U);
        private static readonly Color Pink = new Color(16669612);

        public Color GetColor(string hex)
        {
            if (hex.Contains("#")) hex = hex.Replace("#", "");
            hex = hex.Insert(0, "0x");
            var hexToInt = Convert.ToInt32(hex, 16);
            return new Color((uint) hexToInt);
        }

        public Color GetColor(Colors type)
        {
            Color color;
            switch (type)
            {
                case Colors.Teal:
                    color = Teal;
                    break;
                case Colors.DarkTeal:
                    color = DarkTeal;
                    break;
                case Colors.Green:
                    color = Green;
                    break;
                case Colors.DarkGreen:
                    color = DarkGreen;
                    break;
                case Colors.Blue:
                    color = Blue;
                    break;
                case Colors.DarkBlue:
                    color = DarkBlue;
                    break;
                case Colors.Purple:
                    color = Purple;
                    break;
                case Colors.DarkPurple:
                    color = DarkPurple;
                    break;
                case Colors.Magenta:
                    color = Magenta;
                    break;
                case Colors.DarkMagenta:
                    color = DarkMagenta;
                    break;
                case Colors.Gold:
                    color = Gold;
                    break;
                case Colors.LightOrange:
                    color = LightOrange;
                    break;
                case Colors.Orange:
                    color = Orange;
                    break;
                case Colors.DarkOrange:
                    color = DarkOrange;
                    break;
                case Colors.Red:
                    color = Red;
                    break;
                case Colors.DarkRed:
                    color = DarkRed;
                    break;
                case Colors.LightGrey:
                    color = LightGrey;
                    break;
                case Colors.LighterGrey:
                    color = LighterGrey;
                    break;
                case Colors.DarkGrey:
                    color = DarkGrey;
                    break;
                case Colors.DarkerGrey:
                    color = DarkerGrey;
                    break;
                case Colors.Pink:
                    color = Pink;
                    break;
                default:
                    color = Purple;
                    break;
            }

            return color;
        }
    }

    public enum Colors
    {
        Teal,
        DarkTeal,
        Green,
        DarkGreen,
        Blue,
        DarkBlue,
        Purple,
        DarkPurple,
        Magenta,
        DarkMagenta,
        Gold,
        LightOrange,
        Orange,
        DarkOrange,
        Red,
        DarkRed,
        LightGrey,
        LighterGrey,
        DarkGrey,
        DarkerGrey,
        Pink
    }
}
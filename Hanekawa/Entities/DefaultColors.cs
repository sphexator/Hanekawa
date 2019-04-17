using System;
using Discord;
using Hanekawa.Entities.Interfaces;

namespace Hanekawa.Entities
{
    public class DefaultColors : IHanaService
    {
        private readonly Color _teal = new Color(1752220U);
        private readonly Color _darkTeal = new Color(1146986U);
        private readonly Color _green = new Color(3066993U);
        private readonly Color _darkGreen = new Color(2067276U);
        private readonly Color _blue = new Color(3447003U);
        private readonly Color _darkBlue = new Color(2123412U);
        private readonly Color _purple = new Color(10181046U);
        private readonly Color _darkPurple = new Color(7419530U);
        private readonly Color _magenta = new Color(15277667U);
        private readonly Color _darkMagenta = new Color(11342935U);
        private readonly Color _gold = new Color(15844367U);
        private readonly Color _lightOrange = new Color(12745742U);
        private readonly Color _orange = new Color(15105570U);
        private readonly Color _darkOrange = new Color(11027200U);
        private readonly Color _red = new Color(15158332U);
        private readonly Color _darkRed = new Color(10038562U);
        private readonly Color _lightGrey = new Color(9936031U);
        private readonly Color _lighterGrey = new Color(9807270U);
        private readonly Color _darkGrey = new Color(6323595U);
        private readonly Color _darkerGrey = new Color(5533306U);
        private readonly Color _pink = new Color(16669612);

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
                    color = _teal;
                    break;
                case Colors.DarkTeal:
                    color = _darkTeal;
                    break;
                case Colors.Green:
                    color = _green;
                    break;
                case Colors.DarkGreen:
                    color = _darkGreen;
                    break;
                case Colors.Blue:
                    color = _blue;
                    break;
                case Colors.DarkBlue:
                    color = _darkBlue;
                    break;
                case Colors.Purple:
                    color = _purple;
                    break;
                case Colors.DarkPurple:
                    color = _darkPurple;
                    break;
                case Colors.Magenta:
                    color = _magenta;
                    break;
                case Colors.DarkMagenta:
                    color = _darkMagenta;
                    break;
                case Colors.Gold:
                    color = _gold;
                    break;
                case Colors.LightOrange:
                    color = _lightOrange;
                    break;
                case Colors.Orange:
                    color = _orange;
                    break;
                case Colors.DarkOrange:
                    color = _darkOrange;
                    break;
                case Colors.Red:
                    color = _red;
                    break;
                case Colors.DarkRed:
                    color = _darkRed;
                    break;
                case Colors.LightGrey:
                    color = _lightGrey;
                    break;
                case Colors.LighterGrey:
                    color = _lighterGrey;
                    break;
                case Colors.DarkGrey:
                    color = _darkGrey;
                    break;
                case Colors.DarkerGrey:
                    color = _darkerGrey;
                    break;
                case Colors.Pink:
                    color = _pink;
                    break;
                default:
                    color = _purple;
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
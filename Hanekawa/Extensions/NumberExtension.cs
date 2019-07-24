using System;

namespace Hanekawa.Extensions
{
    public static class NumberExtension
    {
        private static readonly string[] SizeSuffixes =
            {"bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"};

        public static string SizeSuffix(this long value, int decimalPlaces = 1)
        {
            if (decimalPlaces < 0) throw new ArgumentOutOfRangeException(nameof(decimalPlaces));
            if (value < 0) return "-" + SizeSuffix(-value);
            if (value == 0) return string.Format("{0:n" + decimalPlaces + "} bytes", 0);

            var mag = (int) Math.Log(value, 1024);

            var adjustedSize = (decimal) value / (1L << (mag * 10));

            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}",
                adjustedSize,
                SizeSuffixes[mag]);
        }

        public static string FormatNumber(this uint num)
        {
            if (num >= 100000)
                return FormatNumber(num / 1000) + "K";
            if (num >= 10000) return (num / 1000D).ToString("0.#") + "K";
            return num.ToString("#,0");
        }

        public static string FormatNumber(this int num)
        {
            if (num >= 100000)
                return FormatNumber(num / 1000) + "K";
            if (num >= 10000) return (num / 1000D).ToString("0.#") + "K";
            return num.ToString("#,0");
        }
    }
}
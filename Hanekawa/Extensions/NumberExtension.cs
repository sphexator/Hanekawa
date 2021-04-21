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
            switch (value)
            {
                case < 0:
                    return "-" + SizeSuffix(-value);
                case 0:
                    return string.Format("0:n" + decimalPlaces + " bytes", 0);
            }

            var mag = (int) Math.Log(value, 1024);

            var adjustedSize = (decimal) value / (1L << (mag * 10));

            if (Math.Round(adjustedSize, decimalPlaces) < 1000)
                return $"{adjustedSize} {SizeSuffixes[mag]}";
            mag += 1;
            adjustedSize /= 1024;
            return $"{adjustedSize} {SizeSuffixes[mag]}";
        }

        public static bool IsDivisible(this int x, int n) 
            => x != 0 && (n % x) == 0;

        public static string FormatNumber(this int num) 
            => num >= 100000 ? FormatNumber(num / 1000) + "K" :
                num >= 10000 ? (num / 1000D).ToString("0.#") + "K" : num.ToString("#,0");
    }
}
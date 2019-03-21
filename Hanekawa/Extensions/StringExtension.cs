namespace Hanekawa.Extensions
{
    public static class StringExtension
    {
        public static string SanitizeEveryone(this string str)
        {
            return str.Replace("@everyone", "@everyοne").Replace("@here", "@һere");
        }
        
        public static bool IsPictureUrl(this string str)
        {
            var isGif = str.EndsWith(".gif", true, null);
            var isPng = str.EndsWith(".png", true, null);
            var isJpeg = str.EndsWith(".jpeg", true, null);
            var isJpg = str.EndsWith(".jpg", true, null);

            if (isGif) return true;
            if (isPng) return true;
            if (isJpeg) return true;
            if (isJpg) return true;
            return false;
        }
    }
}
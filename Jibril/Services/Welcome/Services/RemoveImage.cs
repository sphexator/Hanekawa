using System.IO;

namespace Jibril.Services.Welcome.Services
{
    public class RemoveImage
    {
        public static void WelcomeFileDelete()
        {
            var banner = new DirectoryInfo(@"Data\Images\Welcome\Cache\Banner\");
            var pfp = new DirectoryInfo(@"Data\Images\Welcome\Cache\Avatar\");
            foreach (var file in banner.GetFiles())
                file.Delete();
            foreach (var file in pfp.GetFiles())
                file.Delete();
        }
    }
}
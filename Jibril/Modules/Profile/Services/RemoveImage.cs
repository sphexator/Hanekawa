using System.IO;

namespace Jibril.Modules.Profile.Services
{
    public class RemoveImage
    {
        public static void RemoveSavedProfile()
        {
            var banner = new DirectoryInfo(@"Data\Images\Profile\Cache\");
            foreach (var file in banner.GetFiles())
                file.Delete();
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Jibril.Services.Welcome.Services
{
    public class RemoveImage
    {
        public static void WelcomeFileDelete()
        {
            DirectoryInfo banner = new DirectoryInfo(@"Data\Images\Welcome\Cache\Banner\");
            DirectoryInfo pfp = new DirectoryInfo(@"Data\Images\Welcome\Cache\Avatar\");
            foreach (FileInfo file in banner.GetFiles())
            {
                file.Delete();
            }
            foreach(FileInfo file in pfp.GetFiles())
            {
                file.Delete();
            }
        }
    }
}

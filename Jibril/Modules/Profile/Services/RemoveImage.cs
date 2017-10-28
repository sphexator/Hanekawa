using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Jibril.Modules.Profile.Services
{
    public class RemoveImage
    {
        public static void RemoveSavedProfile()
        {
            DirectoryInfo banner = new DirectoryInfo(@"Data\Images\Profile\Cache\");
            foreach (FileInfo file in banner.GetFiles())
            {
                file.Delete();
            }        
        }
    }
}

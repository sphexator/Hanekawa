using Discord.Addons.Interactive;
using Discord.Commands;
using Dropbox.Api;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jibril.Modules.Test
{
    public class Dropbox : InteractiveBase
    {
        private const string ApiKey = "jZ6KOHdZXNUAAAAAAABF0nQ_uYsh3edyFHtbwX30n8125jErFFDO7gmv1I3fqr2k";
        private DropboxClient _dropbox;

        [Command("test", RunMode = RunMode.Async)]
        [RequireOwner]
        public async Task SendNudes()
        {
            _dropbox = new DropboxClient(ApiKey);
            //var guild = Context.Guild;

            var Dropboxfolder = "/PicDump";
            try
            {
                var folder = await _dropbox.Files.CreateFolderV2Async(Dropboxfolder);
            }
            catch
            {
                //ignore
            }

            var output = @"Data/Images/PictureSpam";
            //var ch = guild.TextChannels.FirstOrDefault(x => x.Id == 360140270605434882);
            var list = await _dropbox.Files.ListFolderAsync(Dropboxfolder);
            foreach (var item in list.Entries.Where(x => x.IsFile))
            {
                Console.WriteLine($"{output}/{item.Name}");
                var response = await _dropbox.Files.DownloadAsync($"{Dropboxfolder}/{item.Name}", null);
                var file = await response.GetContentAsStreamAsync();
                var fileStream = file as MemoryStream;
                using (FileStream files = new FileStream(output, FileMode.Create, FileAccess.Write))
                {
                    fileStream.WriteTo(files);
                }
                await Context.Channel.SendFileAsync($"{output}/{item.Name}", "");
                await Task.Delay(1500);
            }
        }
    }
}

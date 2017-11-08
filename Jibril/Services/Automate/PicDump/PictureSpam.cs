using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jibril.Services.Automate.PicDump
{
    public class PictureSpam
    {
        private readonly DiscordSocketClient _discord;
        private IServiceProvider _provider;

        public PictureSpam(IServiceProvider provider, DiscordSocketClient discord)
        {
            _discord = discord;
            _provider = provider;
        }

        public Task DownloadFiles()
        {
            var _ = Task.Run(async () =>
            {
                // Connect to Google
                var service = AuthenticateOauth(@"client_secret.json", "test");
                //List the files with the word 'make' in the name.
                var files = List.ListFiles(service);
                foreach (var item in files.Files)
                {
                    // download each file
                    DownloadFile(service, item, string.Format(@"Data\Images\PictureSpam\{0}", item.Name));
                }
                var guild = _discord.GetGuild(339370914724446208);
                DirectoryInfo Pictures = new DirectoryInfo(@"Data\Images\PictureSpam\");
                foreach (FileInfo file in Pictures.GetFiles())
                {
                    var ch = guild.Channels.FirstOrDefault(x => x.Id == 355757410134261760) as SocketTextChannel;
                    await ch.SendFileAsync(@"Data\Images\PictureSpam\" + file.Name, "");
                    await Task.Delay(2500);
                }
                foreach (FileInfo file in Pictures.GetFiles())
                {
                    file.Delete();
                }
            });
            return Task.CompletedTask;
        }

        public static DriveService AuthenticateOauth(string clientSecretJson, string userName)
        {
            try
            {
                if (string.IsNullOrEmpty(userName))
                    throw new ArgumentNullException("userName");
                if (string.IsNullOrEmpty(clientSecretJson))
                    throw new ArgumentNullException("clientSecretJson");
                if (!File.Exists(clientSecretJson))
                    throw new Exception("clientSecretJson file does not exist.");

                // These are the scopes of permissions you need. It is best to request only what you need and not all of them
                string[] scopes = new string[] { DriveService.Scope.DriveReadonly };         	//View the files in your Google Drive                                                 
                UserCredential credential;
                using (var stream = new FileStream(clientSecretJson, FileMode.Open, FileAccess.Read))
                {
                    string credPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                    credPath = Path.Combine(credPath, ".credentials/", System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);

                    // Requesting Authentication or loading previously stored authentication for userName
                    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(GoogleClientSecrets.Load(stream).Secrets,
                                                                             scopes,
                                                                             userName,
                                                                             CancellationToken.None,
                                                                             new FileDataStore(credPath, true)).Result;
                }

                // Create Drive API service.
                return new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Drive Oauth2 Authentication Sample"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Create Oauth2 account DriveService failed" + ex.Message);
                throw new Exception("CreateServiceAccountDriveFailed", ex);
            }
        }

        private static void DownloadFile(DriveService service, Google.Apis.Drive.v3.Data.File file, string saveTo)
        {

            var request = service.Files.Get(file.Id);
            var stream = new MemoryStream();

            // Add a handler which will be notified on progress changes.
            // It will notify on each chunk download and when the
            // download is completed or failed.
            request.MediaDownloader.ProgressChanged += (Google.Apis.Download.IDownloadProgress progress) =>
            {
                switch (progress.Status)
                {
                    case Google.Apis.Download.DownloadStatus.Downloading:
                        {
                            Console.WriteLine(progress.BytesDownloaded);
                            break;
                        }
                    case Google.Apis.Download.DownloadStatus.Completed:
                        {
                            Console.WriteLine("Download complete.");
                            SaveStream(stream, saveTo);
                            break;
                        }
                    case Google.Apis.Download.DownloadStatus.Failed:
                        {
                            Console.WriteLine("Download failed.");
                            break;
                        }
                }
            };
            request.Download(stream);

        }

        private static void SaveStream(MemoryStream stream, string saveTo)
        {
            using (FileStream file = new FileStream(saveTo, FileMode.Create, FileAccess.Write))
            {
                stream.WriteTo(file);
            }
        }
    }
}
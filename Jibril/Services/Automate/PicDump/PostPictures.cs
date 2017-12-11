using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Download;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Quartz;
using File = Google.Apis.Drive.v3.Data.File;

namespace Jibril.Services.Automate.PicDump
{
    public class PostPictures : IJob
    {
        private readonly DiscordSocketClient _discord;
        private IServiceProvider _service;
        private readonly IJobExecutionContext _context;

        public PostPictures(DiscordSocketClient discord, IServiceProvider service)
        {
            _discord = discord;
            _service = service;
            Execute(_context);
        }

        public Task Execute(IJobExecutionContext context)
        {
            Post();
            return Task.CompletedTask;
        }

        private Task Post()
        {
            var _ = Task.Run(async () =>
            {
                try
                {
                    // Connect to Google
                    var service = AuthenticateOauth(@"client_secret.json", "test");
                    //List the files with the word 'make' in the name.
                    var files = List.ListFiles(service);
                    foreach (var item in files.Files)
                        // download each file
                        DownloadFile(service, item, string.Format(@"Data\Images\PictureSpam\{0}", item.Name));
                    var guild = _discord.Guilds.First(x => x.Id == 200265036596379648);
                    var pictures = new DirectoryInfo(@"Data\Images\PictureSpam\");
                    foreach (var file in pictures.GetFiles())
                    {
                        var ch = guild.Channels.FirstOrDefault(x => x.Id == 382890182724157441) as SocketTextChannel;
                        await ch.SendFileAsync(@"Data\Images\PictureSpam\" + file.Name, "");
                        await Task.Delay(2500);
                    }
                    foreach (var file in pictures.GetFiles())
                        file.Delete();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

            });
            return Task.CompletedTask;
        }

        private static DriveService AuthenticateOauth(string clientSecretJson, string userName)
        {
            try
            {
                if (string.IsNullOrEmpty(userName))
                    throw new ArgumentNullException("userName");
                if (string.IsNullOrEmpty(clientSecretJson))
                    throw new ArgumentNullException("clientSecretJson");
                if (!System.IO.File.Exists(clientSecretJson))
                    throw new Exception("clientSecretJson file does not exist.");

                // These are the scopes of permissions you need. It is best to request only what you need and not all of them
                string[]
                    scopes =
                    {
                        DriveService.Scope.DriveReadonly
                    }; //View the files in your Google Drive                                                 
                UserCredential credential;
                using (var stream = new FileStream(clientSecretJson, FileMode.Open, FileAccess.Read))
                {
                    var credPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                    credPath = Path.Combine(credPath, ".credentials/", Assembly.GetExecutingAssembly().GetName().Name);

                    // Requesting Authentication or loading previously stored authentication for userName
                    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(GoogleClientSecrets.Load(stream).Secrets,
                        scopes,
                        userName,
                        CancellationToken.None,
                        new FileDataStore(credPath, true)).Result;
                }

                // Create Drive API service.
                return new DriveService(new BaseClientService.Initializer
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

        private static void DownloadFile(DriveService service, File file, string saveTo)
        {
            var request = service.Files.Get(file.Id);
            var stream = new MemoryStream();

            // Add a handler which will be notified on progress changes.
            // It will notify on each chunk download and when the
            // download is completed or failed.
            request.MediaDownloader.ProgressChanged += progress =>
            {
                switch (progress.Status)
                {
                    case DownloadStatus.Downloading:
                        {
                            Console.WriteLine(progress.BytesDownloaded);
                            break;
                        }
                    case DownloadStatus.Completed:
                        {
                            Console.WriteLine("Download complete.");
                            SaveStream(stream, saveTo);
                            break;
                        }
                    case DownloadStatus.Failed:
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
            using (var file = new FileStream(saveTo, FileMode.Create, FileAccess.Write))
            {
                stream.WriteTo(file);
            }
        }
    }
}
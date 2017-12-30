using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Download;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Jibril.Data.Variables;
using Quartz;
using File = Google.Apis.Drive.v3.Data.File;

namespace Jibril.Services.Automate.PicDump
{
    public class PostPictures : IJob
    {
        private readonly DiscordSocketClient _discord;
        private IServiceProvider _service;

        public PostPictures(DiscordSocketClient discord, IServiceProvider service)
        {
            _discord = discord;
            _service = service;
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
                    var files = List.ListFiles(service, new List.FilesListOptionalParms
                    {
                        PageSize = 1000
                    });
                    foreach (var item in files.Files)
                        // download each file
                        DownloadFile(service, item, string.Format(@"Data\Images\PictureSpam\{0}", item.Name));
                    Console.WriteLine(files.Files.Count);

                    if (files.NextPageToken != null)
                    {
                        var Nextfiles = List.ListFiles(service, new List.FilesListOptionalParms
                        {
                            PageSize = 10,
                            PageToken = files.NextPageToken
                        });
                        Console.WriteLine(Nextfiles.Files.Count);
                        foreach (var item2 in Nextfiles.Files)
                            DownloadFile(service, item2, string.Format(@"Data\Images\PictureSpam\{0}", item2.Name));
                    }

                    var guild = _discord.Guilds.First(x => x.Id == 339370914724446208);
                    //Picdump
                    var ch = guild.Channels.FirstOrDefault(x => x.Id == 355757410134261760) as SocketTextChannel;
                    //Event channel
                    var ech = guild.Channels.First(x => x.Id == 346429829316476928) as SocketTextChannel;

                    var pictures = new DirectoryInfo(@"Data\Images\PictureSpam\");
                    var amount = pictures.GetFiles().Length;
                    var eventMsg = DefaultEmbed(amount);
                    var updEMsg = UpdateEmbed(eventMsg, amount);

                    var emsg = await ech.SendMessageAsync("", false, eventMsg.Build());


                    foreach (var file in pictures.GetFiles())
                        try
                        {
                            await ch.SendFileAsync(@"Data\Images\PictureSpam\" + file.Name, "");
                            await Task.Delay(5000);
                        }
                        catch
                        {
                            // Ignore
                        }
                    try
                    {
                        CompressFiles();
                        await Task.Delay(2000);
                        var finalMsg = await ch.SendFileAsync(@"Data\Images\PictureSpam\result.zip", "");
                        await finalMsg.PinAsync();
                    }
                    catch
                    {
                        // Ignore
                    }
                    await Task.Delay(250);
                    await emsg.ModifyAsync(m => m.Embed = updEMsg.Build());

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

        private static EmbedBuilder DefaultEmbed(int amount)
        {
            var content = $"Picture dump event \n" +
                          $"These pictures are both NSFW & SFW. A zip file with all pics will be linked at the end. \n" +
                          $"\n" +
                          $"To gain access to the picture dump channel, head to #roles and do !picdump";
            var embed = new EmbedBuilder
            {
                Color = new Color(Colours.DefaultColour),
                Description = content
            };
            var status = new EmbedFieldBuilder
            {
                IsInline = true,
                Name = "Status",
                Value = "In progress"
            };
            var picAmount = new EmbedFieldBuilder
            {
                IsInline = true,
                Name = "Amount",
                Value = $"{amount}"
            };
            embed.AddField(status);
            embed.AddField(picAmount);
            return embed;
        }

        private static EmbedBuilder UpdateEmbed(EmbedBuilder embed, int amount)
        {
            var updEmbed = new EmbedBuilder
            {
                Color = embed.Color,
                Description = embed.Description
            };
            var status = new EmbedFieldBuilder
            {
                IsInline = true,
                Name = "Status",
                Value = "Done"
            };
            var picAmount = new EmbedFieldBuilder
            {
                IsInline = true,
                Name = "Amount",
                Value = $"{amount}"
            };
            updEmbed.AddField(status);
            updEmbed.AddField(picAmount);
            return updEmbed;
        }

        private void CompressFiles()
        {
            var startPath = @"Data\Images\PictureSpam\";
            var zipPath = @"Data\Images\PictureSpam\result.zip";
            ZipFile.CreateFromDirectory(startPath, zipPath);
        }
    }
}
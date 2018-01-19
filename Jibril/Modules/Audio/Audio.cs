using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Audio;
using Discord.Commands;
using Jibril.Modules.Audio.Service;
using Jibril.Preconditions;

namespace Jibril.Modules.Audio
{
    public class Audio : InteractiveBase
    {   
        [Command("play", RunMode = RunMode.Async)]
        [RequireOwner]
        public async Task Play(string url)
        {
            var channel = (Context.User as IVoiceState)?.VoiceChannel;
            var client = await channel.ConnectAsync();

            var output = CreateStream(url).StandardOutput.BaseStream;
            var stream = client.CreatePCMStream(AudioApplication.Music);
            await output.CopyToAsync(stream);
            await stream.FlushAsync().ConfigureAwait(false);
        }

        private static Process CreateStream(string url)
        {
            var currentsong = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C youtube-dl.exe -o - {url} | ffmpeg -i pipe:0 -ac 2 -f s16le -af volume=0.5 -ar 48000 pipe:1",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = false
                }
            };
            currentsong.Start();
            return currentsong;
        }
    }
}

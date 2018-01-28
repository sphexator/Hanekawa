﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

// Change this namespace if desired
namespace Jibril.Services.Reliability
{
    // This service requires that your bot is being run by a daemon that handles
    // Exit Code 1 (or any exit code) as a restart.
    //
    // If you do not have your bot setup to run in a daemon, this service will just
    // terminate the process and the bot will not restart.
    // 
    // Links to daemons:
    // [Powershell (Windows+Unix)] https://gitlab.com/snippets/21444
    // [Bash (Unix)] https://stackoverflow.com/a/697064
    public class ReliabilityService
    {
        private static readonly TimeSpan _timeout = TimeSpan.FromSeconds(30);
        private static readonly bool _attemptReset = true;

        private static readonly LogSeverity _debug = LogSeverity.Debug;
        private static readonly LogSeverity _info = LogSeverity.Info;
        private static readonly LogSeverity _critical = LogSeverity.Critical;

        private readonly DiscordSocketClient _discord;
        private readonly Func<LogMessage, Task> _logger;
        private CancellationTokenSource _cts;

        public ReliabilityService(DiscordSocketClient discord, Func<LogMessage, Task> logger = null)
        {
            _cts = new CancellationTokenSource();
            _discord = discord;
            _logger = logger ?? (_ => Task.CompletedTask);

            _discord.Connected += ConnectedAsync;
            _discord.Disconnected += DisconnectedAsync;
        }

        public Task ConnectedAsync()
        {
            _ = DebugAsync("Client reconnected, resetting cancel tokens...");
            _cts.Cancel();
            _cts = new CancellationTokenSource();
            _ = DebugAsync("Client reconnected, cancel tokens reset.");

            return Task.CompletedTask;
        }

        public Task DisconnectedAsync(Exception _e)
        {
            _ = InfoAsync("Client disconnected, starting timeout task...");
            _ = Task.Delay(_timeout, _cts.Token).ContinueWith(async _ =>
            {
                await DebugAsync("Timeout expired, continuing to check client state...");
                await CheckStateAsync();
                await DebugAsync("State came back okay");
            });

            return Task.CompletedTask;
        }

        private async Task CheckStateAsync()
        {
            if (_discord.ConnectionState == ConnectionState.Connected) return;
            if (_attemptReset)
            {
                await InfoAsync("Attempting to reset the client");

                var timeout = Task.Delay(_timeout);
                var connect = _discord.StartAsync();
                var task = await Task.WhenAny(timeout, connect);

                if (task == timeout)
                {
                    await CriticalAsync("Client reset timed out (task deadlocked?), killing process");
                    FailFast();
                }
                else if (connect.IsFaulted)
                {
                    await CriticalAsync("Client reset faulted, killing process", connect.Exception);
                    FailFast();
                }
                else if (connect.IsCompletedSuccessfully)
                    await InfoAsync("Client reset succesfully!");
            }

            await CriticalAsync("Client did not reconnect in time, killing process");
            FailFast();
        }

        private void FailFast()
            => Environment.Exit(1);

        private const string LogSource = "Reliability";
        private Task DebugAsync(string message)
            => _logger.Invoke(new LogMessage(_debug, LogSource, message));
        private Task InfoAsync(string message)
            => _logger.Invoke(new LogMessage(_info, LogSource, message));
        private Task CriticalAsync(string message, Exception error = null)
            => _logger.Invoke(new LogMessage(_critical, LogSource, message, error));
    }
}
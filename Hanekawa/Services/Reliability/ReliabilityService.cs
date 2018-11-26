using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Services.Logging;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Services.Reliability
{
    public class ReliabilityService
    {
        private const bool AttemptReset = true;

        private const string LogSource = "Reliability";
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(30);

        private readonly DiscordSocketClient _discord;
        private readonly LogService _logger;
        private readonly Timer _timer;
        private CancellationTokenSource _cts;

        public ReliabilityService(DiscordSocketClient discord, LogService logger)
        {
            _cts = new CancellationTokenSource();
            _discord = discord;
            _logger = logger;

            _discord.Connected += ConnectedAsync;
            _discord.Disconnected += DisconnectedAsync;

            _timer = new Timer(__ =>
            {
                if (_discord.ConnectionState != ConnectionState.Disconnected ||
                    _discord.ConnectionState != ConnectionState.Connecting) return;
                __ = InfoAsync("Client disconnected, starting timeout task...");
                __ = Task.Delay(Timeout, _cts.Token).ContinueWith(async ___ =>
                {
                    await DebugAsync("Timeout expired, continuing to check client state...");
                    await CheckStateAsync();
                    await DebugAsync("State came back okay");
                });
            }, null, TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(2));
            Console.WriteLine("Reliability service loaded");
        }

        private Task ConnectedAsync()
        {
            var __ = Task.Run(() =>
            {
                _ = DebugAsync("Client reconnected, resetting cancel tokens...");
                _cts.Cancel();
                _cts = new CancellationTokenSource();
                _ = DebugAsync("Client reconnected, cancel tokens reset.");
            });
            return Task.CompletedTask;
        }

        private Task DisconnectedAsync(Exception _e)
        {
            var __ = Task.Run(() =>
            {
                _ = InfoAsync("Client disconnected, starting timeout task...");
                _ = Task.Delay(Timeout, _cts.Token).ContinueWith(async _ =>
                {
                    await DebugAsync("Timeout expired, continuing to check client state...");
                    await CheckStateAsync();
                    await DebugAsync("State came back okay");
                });
            });
            return Task.CompletedTask;
        }

        private async Task CheckStateAsync()
        {
            if (_discord.ConnectionState == ConnectionState.Connected) return;

            if (AttemptReset)
            {
                await InfoAsync("Attempting to reset the client");

                var timeout = Task.Delay(Timeout);
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
                {
                    await InfoAsync("Client reset succesfully!");
                }
            }

            await CriticalAsync("Client did not reconnect in time, killing process");
            FailFast();
        }

        private static void FailFast()
        {
            Environment.Exit(1);
        }

        private Task DebugAsync(string message)
        {
            _logger.LogAction(LogLevel.Debug, message, LogSource);
            return Task.CompletedTask;
        }

        private Task InfoAsync(string message)
        {
            _logger.LogAction(LogLevel.Information, message, LogSource);
            return Task.CompletedTask;
        }

        private Task CriticalAsync(string message, Exception error = null)
        {
            _logger.LogAction(LogLevel.Critical, message, error?.Message ?? LogSource);
            return Task.CompletedTask;
        }
    }
}
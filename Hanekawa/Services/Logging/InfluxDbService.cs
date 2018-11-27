using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.WebSocket;
using Hanekawa.Entities.Interfaces;
using InfluxData.Net.Common.Enums;
using InfluxData.Net.InfluxDb;
using InfluxData.Net.InfluxDb.Models;

namespace Hanekawa.Services.Logging
{
    public class InfluxDbService : IHanaService, IRequiredService
    {
        private readonly DiscordSocketClient _client;
        private readonly InfluxDbClient _influx;

        public InfluxDbService(DiscordSocketClient client)
        {
            _client = client;

            _client.Connected += OnClientConnected;
            _client.Disconnected += OnClientDisconnected;
            _client.JoinedGuild += OnClientJoinedGuild;
            _client.LeftGuild += OnClientLeftGuild;
            _client.UserLeft += OnUserLeft;
            _client.UserJoined += OnUserJoined;
            _client.MessageReceived += OnMessageReceived;
            _client.UserBanned += OnUserBanned;
            _client.UserUnbanned += OnUserUnbanned;

            _influx = new InfluxDbClient("http://localhost:8086/", "username", "password", InfluxDbVersion.Latest);
            Console.WriteLine("InfluxDb service loaded");
        }

        private Task OnMessageReceived(SocketMessage message)
        {
            var _ = Task.Run(async () =>
            {
                if (!(message.Author is SocketGuildUser user)) return;
                try
                {
                    var pointToWrite = new Point
                    {
                        Name = "GldMsg",
                        Tags = new Dictionary<string, object>
                        {
                            {"GuildId", user.Guild.Id}
                        },
                        Fields = new Dictionary<string, object>
                        {
                            {$"{user.Guild.Id}", 1}
                        },
                        Timestamp = DateTime.UtcNow
                    };
                    await _influx.Client.WriteAsync(pointToWrite, "Hanekawa");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });
            return Task.CompletedTask;
        }

        private Task OnUserUnbanned(SocketUser user, SocketGuild guild)
        {
            var _ = Task.Run(async () =>
            {
                try
                {
                    var pointToWrite = new Point
                    {
                        Name = "Moderator",
                        Tags = new Dictionary<string, object>
                        {
                            {"GuildId", guild.Id}
                        },
                        Fields = new Dictionary<string, object>
                        {
                            {$"unb_{guild.Id}", $"{user.Id}"}
                        },
                        Timestamp = DateTime.UtcNow
                    };
                    await _influx.Client.WriteAsync(pointToWrite, "Hanekawa");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });
            return Task.CompletedTask;
        }

        private Task OnUserBanned(SocketUser user, SocketGuild guild)
        {
            var _ = Task.Run(async () =>
            {
                try
                {
                    var pointToWrite = new Point
                    {
                        Name = "Moderator",
                        Tags = new Dictionary<string, object>
                        {
                            {"GuildId", guild.Id}
                        },
                        Fields = new Dictionary<string, object>
                        {
                            {$"ban_{guild.Id}", $"{user.Id}"}
                        },
                        Timestamp = DateTime.UtcNow
                    };
                    await _influx.Client.WriteAsync(pointToWrite, "Hanekawa");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });
            return Task.CompletedTask;
        }

        private Task OnUserJoined(SocketGuildUser user)
        {
            var _ = Task.Run(async () =>
            {
                try
                {
                    var pointToWrite = new Point
                    {
                        Name = "MemberStat",
                        Fields = new Dictionary<string, object>
                        {
                            {$"Join-{user.Guild.Id}", $"{user.Id}"}
                        },
                        Tags = new Dictionary<string, object>
                        {
                            {"GuildId", $"{user.Id}"}
                        },
                        Timestamp = DateTime.UtcNow
                    };
                    await _influx.Client.WriteAsync(pointToWrite, "Hanekawa");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });
            return Task.CompletedTask;
        }

        private Task OnUserLeft(SocketGuildUser user)
        {
            var _ = Task.Run(async () =>
            {
                try
                {
                    var pointToWrite = new Point
                    {
                        Name = "MemberStat",
                        Tags = new Dictionary<string, object>
                        {
                            {"GuildId", user.Guild.Id}
                        },
                        Fields = new Dictionary<string, object>
                        {
                            {$"Left-{user.Guild.Id}", $"{user.Id}"}
                        },
                        Timestamp = DateTime.UtcNow
                    };
                    await _influx.Client.WriteAsync(pointToWrite, "Hanekawa");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });
            return Task.CompletedTask;
        }

        private Task OnClientLeftGuild(SocketGuild guild)
        {
            var _ = Task.Run(async () =>
            {
                try
                {
                    var pointToWrite = new Point
                    {
                        Name = "Guild",
                        Tags = new Dictionary<string, object>
                        {
                            {"GuildId", guild.Id}
                        },
                        Fields = new Dictionary<string, object>
                        {
                            {"Left", 1}
                        },
                        Timestamp = DateTime.UtcNow
                    };
                    await _influx.Client.WriteAsync(pointToWrite, "Hanekawa");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });
            return Task.CompletedTask;
        }

        private Task OnClientJoinedGuild(SocketGuild guild)
        {
            var _ = Task.Run(async () =>
            {
                try
                {
                    var pointToWrite = new Point
                    {
                        Name = "Guild",
                        Tags = new Dictionary<string, object>
                        {
                            {"GuildId", guild.Id}
                        },
                        Fields = new Dictionary<string, object>
                        {
                            {"Join", 1}
                        },
                        Timestamp = DateTime.UtcNow
                    };
                    await _influx.Client.WriteAsync(pointToWrite, "Hanekawa");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });
            return Task.CompletedTask;
        }

        private Task OnClientDisconnected(Exception arg)
        {
            var _ = Task.Run(async () =>
            {
                try
                {
                    var pointToWrite = new Point
                    {
                        Name = "Bot",
                        Tags = new Dictionary<string, object>
                        {
                            {"Disconnect", 1}
                        },
                        Fields = new Dictionary<string, object>
                        {
                            {"Disconnect", 1}
                        },
                        Timestamp = DateTime.UtcNow
                    };
                    await _influx.Client.WriteAsync(pointToWrite, "Hanekawa");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });
            return Task.CompletedTask;
        }

        private Task OnClientConnected()
        {
            var _ = Task.Run(async () =>
            {
                try
                {
                    var pointToWrite = new Point
                    {
                        Name = "Bot",
                        Tags = new Dictionary<string, object>
                        {
                            {"Connect", 1}
                        },
                        Fields = new Dictionary<string, object>
                        {
                            {"Connect", 1}
                        },
                        Timestamp = DateTime.UtcNow
                    };
                    await _influx.Client.WriteAsync(pointToWrite, "Hanekawa");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });
            return Task.CompletedTask;
        }
    }
}
using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.Lavalink;
using DSharpPlus.Net;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Services
{
    public class LavalinkService
    {
        public LavalinkNodeConnection LavalinkNode { get; private set; }

        private DiscordClient Client { get; }

        public LavalinkService(DiscordClient client)
        {
            Client = client;

            client.Ready += Client_Ready;
        }

        private Task Client_Ready(DiscordClient sender, ReadyEventArgs e)
        {
            if (LavalinkNode != null)
                return Task.CompletedTask;

            Task.Run(async () =>
            {
                LavalinkExtension lavalink = Client.GetLavalink();

                ConnectionEndpoint endpoint = new ConnectionEndpoint // TODO: Replace constants with config file data
                {
                    Hostname = "127.0.0.1", 
                    Port = 2333
                };

                LavalinkConfiguration lavaConfig = new LavalinkConfiguration
                {
                    Password = "youshallnotpass", 
                    RestEndpoint = endpoint,
                    SocketEndpoint = endpoint
                };

                LavalinkNode = await lavalink.ConnectAsync(lavaConfig);
            });

            return Task.CompletedTask;
        }
    }
}

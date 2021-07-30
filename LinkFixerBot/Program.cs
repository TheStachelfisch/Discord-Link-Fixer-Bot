using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace LinkFixerBot
{
    class Program
    {
        public readonly string GatewayToken = File.ReadAllText("token.ignore");
        private DiscordSocketClient _client;

        static void Main() => new Program().StartAsync().GetAwaiter().GetResult();

        public async Task StartAsync()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Info,
                ConnectionTimeout = 30000,
                HandlerTimeout = 5000,
                DefaultRetryMode = RetryMode.RetryRatelimit,
                UseSystemClock = true
            });

            _client.Log += message =>
            {
                Console.WriteLine(message.ToString());
                return Task.CompletedTask;
            };
            
            await _client.LoginAsync(TokenType.Bot, GatewayToken);
            await _client.StartAsync();

            Task ReadyEvent()
            {
                _client.Ready -= ReadyEvent;
                
                SetupAsync().GetAwaiter().GetResult();

                return Task.CompletedTask;
            }

            _client.Ready += ReadyEvent;
            
            await Task.Delay(-1);
        }

        private Task SetupAsync()
        {
            _client.MessageReceived += FixMessageHandler;
            return Task.CompletedTask;
        }

        private async Task FixMessageHandler(SocketMessage message)
        {
            var brokenLink = message.Embeds.FirstOrDefault(x => x.Type == EmbedType.Video)?.Url;
            if (brokenLink == null)
                return;
            
            
            if (brokenLink.Contains("https://media.discordapp.net/attachments/") || brokenLink.Contains("http://media.discordapp.net/attachments/") && !message.Author.IsBot)
                await ((IUserMessage) message).ReplyAsync(brokenLink.ReplaceFirst("media", "cdn").ReplaceFirst(".net", ".com"), allowedMentions: AllowedMentions.None);
        }
    }
}
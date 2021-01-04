using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using System.Text.Json;
using QP.Config;
using System.IO;
using QP.Mods;

namespace QP
{
    public class Program
    {
        private DiscordSocketClient _client;

        private RoleMod roleMod;

        private RoleConfig roleConfig;

        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            string token = Environment.GetEnvironmentVariable("token");

            var config = new DiscordSocketConfig
            {
                AlwaysDownloadUsers = true,
                MessageCacheSize = 100,
            };

            _client = new DiscordSocketClient(config);

            _client.Log += Log;
            _client.Ready += Init;

            roleConfig = JsonSerializer.Deserialize<RoleConfig>(File.ReadAllText(@"config\roles.json"));

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        private Task Init()
        {
            roleMod = new RoleMod(roleConfig, _client);
            roleMod.LoadRoles();
            return Task.CompletedTask;
        }

        // Move to Logging Object.
        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}

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
                AlwaysDownloadUsers = true, // Should grab users, doesn't seem to be...
                MessageCacheSize = 100, // Let's start with a cache of this size
            };

            _client = new DiscordSocketClient(config);

            _client.Log += Log; // Register Log Event
            _client.Ready += Init; // Register Ready Event

            // Load the roles from file
            roleConfig = JsonSerializer.Deserialize<RoleConfig>(File.ReadAllText(@"config\roles.json"));

            // Login and star the bot
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        /// <summary>
        /// Runs after guild data has become available
        /// </summary>
        /// <returns>Task</returns>
        private Task Init()
        {
            // Initialize the RoleMod and load in roles
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

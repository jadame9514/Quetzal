using System.Security.Principal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using QP.Config;

namespace QP.Mods
{
    public class RoleMod
    {
        private readonly DiscordSocketClient client;
        private readonly RoleConfig roleConfig;
        private Dictionary<string, Dictionary<string, IRole>> roleMessagesMaps;

        public RoleMod(RoleConfig roleConfig, DiscordSocketClient client)
        {
            client.ReactionAdded += ReactionAdded;
            this.client = client;
            this.roleConfig = roleConfig;
        }

        /// <summary>
        /// Convert the messages and emoji to role maps to utilize the IRole type and confirm
        /// these roles actually exist on the server
        /// </summary>
        /// <returns></returns>
        public bool LoadRoles()
        {
            // There's probably a more elegant way to do this...
            try
            {
                roleMessagesMaps = new Dictionary<string, Dictionary<string, IRole>>();
                foreach (KeyValuePair<string, IDictionary<string, string>> roleMessageMap in roleConfig.RoleMessages)
                {
                    var emojiRoleMap = new Dictionary<string, IRole>();
                    foreach (KeyValuePair<string, string> emojiToRoleMap in roleMessageMap.Value)
                    {
                        // Convert the Role Name to the actual Role type used by Discord.Net, this will be what is assigned to the users
                        emojiRoleMap.Add(emojiToRoleMap.Key, client.GetGuild(roleConfig.ServerId).Roles.First(r => r.Name.Equals(emojiToRoleMap.Value)) as IRole);
                    }

                    roleMessagesMaps.Add(roleMessageMap.Key, emojiRoleMap);
                }
            }
            catch (Exception e)
            {
                // TODO: Don't just swallow exception, log the problem
                client.GetGuild(roleConfig.ServerId).GetTextChannel(roleConfig.RoleChannelId).SendMessageAsync("Father, roles are not configured properly...");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Function to register to the BaseSocketClient ReactionAdded event
        /// </summary>
        /// <param name="cachedMessage">The message the reaction is on</param>
        /// <param name="originChannel">The channel the reaction is in</param>
        /// <param name="reaction">The reaction</param>
        /// <returns></returns>
        private async Task ReactionAdded(Cacheable<IUserMessage, ulong> cachedMessage, ISocketMessageChannel originChannel, SocketReaction reaction)
        {
            /* TODO: Turn into helper method and reuse for ReactionRemoved *****************************************************/
            Dictionary<string, IRole> rolesMap;
            // Only care about reactions on the role message.
            if (!roleMessagesMaps.TryGetValue(cachedMessage.Id.ToString(), out rolesMap) /*&& !reaction.User.Value.IsBot*/) return;

            IRole roleToSet;
            if (reaction.Emote as Emote == null)
            {
                // Handle standard emoji
                rolesMap.TryGetValue(reaction.Emote.Name, out roleToSet);
            }
            else
            {
                // Handle custom emoji
                rolesMap.TryGetValue((reaction.Emote as Emote).Id.ToString(), out roleToSet);
            }

            if (roleToSet != null)
            {
                var user = await client.Rest.GetGuildUserAsync(roleConfig.ServerId, reaction.UserId); // This is bullshit, fetch the user via REST call
                await user.AddRoleAsync(roleToSet); // Add the role to the user
            }
            else
            {
                // TODO: Probably some other emoji that was added, remove it.
            }
            /******************************************************************************************************************/
        }

        /// <summary>
        /// Function to register to the BaseSocketClient ReactionRemoved event
        /// </summary>
        /// <param name="cachedMessage">The message the reaction is on</param>
        /// <param name="originChannel">The channel the reaction is in</param>
        /// <param name="reaction">The reaction</param>
        /// <returns></returns>
        private async Task ReactionRemoved(Cacheable<IUserMessage, ulong> cachedMessage, ISocketMessageChannel originChannel, SocketReaction reaction)
        {
            // TODO: Remove the role
        }

        private async Task ResetColors()
        {
            // TODO: Method to clear color roles from everyone
        }
    }
}
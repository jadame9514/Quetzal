using System.Collections.Generic;
namespace QP.Config
{
    public class RoleConfig
    {
        public ulong ServerId { get; set; }
        public ulong RoleChannelId { get; set; }
        public IDictionary<string, IDictionary<string, string>> RoleMessages { get; set; }
    }
}

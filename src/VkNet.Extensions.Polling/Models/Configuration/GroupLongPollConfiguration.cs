using System;
using VkNet.Enums.StringEnums;

namespace VkNet.Extensions.Polling.Models.Configuration
{
    public class GroupLongPollConfiguration : ILongPollConfiguration
    {
        public static GroupLongPollConfiguration Default => new GroupLongPollConfiguration()
        {
            RequestDelay = TimeSpan.FromMilliseconds(333),
            WaitMs = 25,
            IgnorePreviousUpdates = true
        };
        
        public TimeSpan RequestDelay { get; set; }

        public int WaitMs { get; set; }
        
        public bool IgnorePreviousUpdates { get; set; }

        public GroupUpdateType[] AllowedUpdateTypes { get; set;  }
    }
}
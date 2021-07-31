using System;
using System.Collections.Generic;
using VkNet.Enums.SafetyEnums;

namespace VkNet.Extensions.Polling.Models.Configuration
{
    public class GroupLongPollConfiguration : ILongPollConfiguration
    {
        public static GroupLongPollConfiguration Default => new GroupLongPollConfiguration()
        {
            RequestDelay = TimeSpan.FromMilliseconds(333),
            IgnorePreviousUpdates = true
        };
        
        public TimeSpan RequestDelay { get; set; }

        public bool IgnorePreviousUpdates { get; set; }

        public GroupUpdateType[] AllowedUpdateTypes { get; set;  }
    }
}
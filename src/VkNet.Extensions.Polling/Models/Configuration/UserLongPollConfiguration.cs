using System;
using VkNet.Enums.Filters;
using VkNet.Enums.SafetyEnums;

namespace VkNet.Extensions.Polling.Models.Configuration
{
    public class UserLongPollConfiguration : ILongPollConfiguration
    {
        public static UserLongPollConfiguration Default => new UserLongPollConfiguration()
        {
            RequestDelay = TimeSpan.FromMilliseconds(333),
            IgnorePreviousUpdates = true
        };
        
        public TimeSpan RequestDelay { get; set; }
        public bool IgnorePreviousUpdates { get; set; }
        public UsersFields Fields { get; set; } 
        
    }
}
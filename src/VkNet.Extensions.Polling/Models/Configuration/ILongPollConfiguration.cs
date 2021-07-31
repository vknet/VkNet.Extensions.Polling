using System;

namespace VkNet.Extensions.Polling.Models.Configuration
{
    public interface ILongPollConfiguration
    {
        
        TimeSpan RequestDelay { get; }

        bool IgnorePreviousUpdates { get; }
    }
}
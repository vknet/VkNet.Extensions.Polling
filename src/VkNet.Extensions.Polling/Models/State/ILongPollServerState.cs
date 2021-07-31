namespace VkNet.Extensions.Polling.Models.State
{
    public interface ILongPollServerState
    {
        ulong Ts { get; }
        
        void Update(ulong value);
    }
}
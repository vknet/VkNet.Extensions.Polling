namespace VkNet.Extensions.Polling.Models.State
{
    public class UserLongPollServerState : ILongPollServerState
    {
        public UserLongPollServerState(ulong ts, ulong pts)
        {
            Ts = ts;
            Pts = pts;
        }

        public ulong Ts { get; }
        public ulong Pts { get; private set; }

        public void Update(ulong pts)
        {
            Pts = pts;
        }
    }
}
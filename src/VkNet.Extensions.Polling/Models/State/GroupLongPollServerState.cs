namespace VkNet.Extensions.Polling.Models.State
{
    public class GroupLongPollServerState : ILongPollServerState
    {
        public GroupLongPollServerState(ulong ts, string key, string server)
        {
            Ts = ts;
            Key = key;
            Server = server;
        }

        public ulong Ts { get; private set; }
        public string Key { get; }
        public string Server { get; }

        public void Update(ulong ts)
        {
            Ts = ts;
        }
    }
}
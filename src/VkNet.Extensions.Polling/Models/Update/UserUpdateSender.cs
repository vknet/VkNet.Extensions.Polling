using VkNet.Extensions.Polling.Enums;
using VkNet.Model;

namespace VkNet.Extensions.Polling.Models.Update
{
    public class UserUpdateSender
    {
        public UserUpdateSender(Group group)
        {
            Type = UpdateSenderType.Group;
            Group = group;
        }

        public UserUpdateSender(User user)
        {
            Type = UpdateSenderType.User;
            User = user;
        }

        public UpdateSenderType Type { get; }

        public Group Group { get; }

        public User User { get; }
    }
}
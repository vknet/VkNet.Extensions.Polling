using VkNet.Model;

namespace VkNet.Extensions.Polling.Models.Update
{
    public class UserUpdate
    {
        public UserUpdate(Message message, UserUpdateSender userUpdateSender)
        {
            Message = message;
            Sender = userUpdateSender;
        }

        public Message Message { get; }

        public UserUpdateSender Sender { get; }
    }
}
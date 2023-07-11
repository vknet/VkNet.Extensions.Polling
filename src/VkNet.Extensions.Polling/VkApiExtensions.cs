using System;
using System.Threading;
using VkNet.Extensions.Polling.Models.Configuration;

namespace VkNet.Extensions.Polling
{
    public static class VkApiExtensions
    {
        /// <summary>
        /// Запускает Лонг Пулл для Пользователя.
        /// </summary>
        /// <param name="vkApi">Экземпляр VkApi.</param>
        /// <param name="userLongPollConfiguration">Конфигурация Лонг Пулла.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static UserLongPoll StartUserLongPollAsync(this VkApi vkApi, UserLongPollConfiguration userLongPollConfiguration, CancellationToken cancellationToken = default)
        {
            var userLongPoll = new UserLongPoll(vkApi);
            
            userLongPoll.Start(userLongPollConfiguration, cancellationToken).ConfigureAwait(false);

            return userLongPoll;
        }

        /// <summary>
        /// Запускает Лонг Пулл для Группы.
        /// </summary>
        /// <param name="vkApi">Экземпляр VkApi.</param>
        /// <param name="groupLongPollConfiguration">Конфигурация Лонг Пулла.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static GroupLongPoll StartGroupLongPollAsync(this VkApi vkApi, GroupLongPollConfiguration groupLongPollConfiguration, CancellationToken cancellationToken = default)
        {
            var groupLongPoll = new GroupLongPoll(vkApi);
            
            groupLongPoll.Start(groupLongPollConfiguration, cancellationToken).ConfigureAwait(false);

            return groupLongPoll;
        }
        
        public static bool IsAuthorizedAsUser(this VkApi api)
        {
            try
            {
                var userLongPoll = new UserLongPoll(api);

                return true;
            }
            catch (NotSupportedException)
            {
                return false;
            }
        }
        
        public static bool IsAuthorizedAsGroup(this VkApi api)
        {
            try
            {
                var userLongPoll = new GroupLongPoll(api);

                return true;
            }
            catch (NotSupportedException)
            {
                return false;
            }
        }
        
    }
    
}
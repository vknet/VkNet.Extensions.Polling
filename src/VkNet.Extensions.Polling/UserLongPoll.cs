using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VkNet.Abstractions;
using VkNet.Extensions.Polling.Models.Configuration;
using VkNet.Extensions.Polling.Models.State;
using VkNet.Extensions.Polling.Models.Update;
using VkNet.Model;

namespace VkNet.Extensions.Polling
{
    public class UserLongPoll :
        LongPollBase<LongPollHistoryResponse, UserUpdate, UserLongPollServerState, UserLongPollConfiguration>
    {
        public UserLongPoll(IVkApi vkApi) : base(vkApi)
        {
        }

        protected override bool Validate(IVkApi vkApi)
        {
            return vkApi.Users.Get(new long[] { }).Any();
        }

        protected override async Task<UserLongPollServerState> GetServerInformationAsync(IVkApi vkApi,
            UserLongPollConfiguration longPollConfiguration, CancellationToken cancellationToken = default)
        {
            return await vkApi.Messages.GetLongPollServerAsync(true, token: cancellationToken)
                .ContinueWith(
                    _ => new UserLongPollServerState(Convert.ToUInt64(_.Result.Ts),
                        _.Result?.Pts ??
                        throw new InvalidOperationException(
                            "Не удалось получить Pts. Проблема при получении информации о сервере.")),
                    cancellationToken);
        }

        protected override Task<LongPollHistoryResponse> GetUpdatesAsync(IVkApi vkApi,
            UserLongPollConfiguration userLongPollConfiguration,
            UserLongPollServerState longPollServerInformation,
            CancellationToken cancellationToken = default)
        {
            return vkApi.Messages.GetLongPollHistoryAsync(new MessagesGetLongPollHistoryParams
            {
                Pts = longPollServerInformation.Pts,
                Ts = longPollServerInformation.Ts,
                Fields = userLongPollConfiguration.Fields
            }, cancellationToken).ContinueWith(_ =>
            {
                longPollServerInformation.Update(_.Result.NewPts);

                return _.Result;
            }, cancellationToken);
        }

        protected override IEnumerable<UserUpdate> ConvertLongPollResponse(
            LongPollHistoryResponse longPollResponse)
        {
            return from message in longPollResponse.Messages
                let updateSender =
                    message.FromId < 0
                        ? new UserUpdateSender(longPollResponse.Groups.First(_ => _.Id == message.FromId))
                        : new UserUpdateSender(longPollResponse.Profiles.First(_ => _.Id == message.FromId))
                select new UserUpdate(message, updateSender);
        }
    }
}
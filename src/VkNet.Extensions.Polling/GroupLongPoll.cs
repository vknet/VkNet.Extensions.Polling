using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VkNet.Abstractions;
using VkNet.Extensions.Polling.Models.Configuration;
using VkNet.Extensions.Polling.Models.State;
using VkNet.Model;
using VkNet.Model.GroupUpdate;
using VkNet.Model.RequestParams;

namespace VkNet.Extensions.Polling
{
    public class GroupLongPoll :
        LongPollBase<BotsLongPollHistoryResponse, GroupUpdate, GroupLongPollServerState,
            GroupLongPollConfiguration>
    {
        private long _groupId;

        public GroupLongPoll(IVkApi vkApi) : base(vkApi)
        {
        }

        protected override bool Validate(IVkApi vkApi)
        {
            var groups = vkApi.Groups.GetById(null, null, null);

            var groupOwner = groups.FirstOrDefault();
            
            if (groupOwner != null)
            {
                _groupId = groupOwner.Id;
                
                return true;
            }

            return false;
        }

        protected override async Task<GroupLongPollServerState> GetServerInformationAsync(IVkApi vkApi,
            GroupLongPollConfiguration longPollConfiguration, CancellationToken cancellationToken = default)
        {
            return await vkApi.Groups.GetLongPollServerAsync(Convert.ToUInt64(_groupId))
                .ContinueWith(_ =>
                {
                    return new GroupLongPollServerState(
                        Convert.ToUInt64(_.Result.Ts),
                        _.Result.Key,
                        _.Result.Server
                    );
                }, cancellationToken);
        }

        protected override Task<BotsLongPollHistoryResponse> GetUpdatesAsync(IVkApi vkApi,
            GroupLongPollConfiguration longPollConfiguration,
            GroupLongPollServerState longPollServerInformation,
            CancellationToken cancellationToken = default)
        {
            return vkApi.Groups.GetBotsLongPollHistoryAsync(new BotsLongPollHistoryParams
            {
                Key = longPollServerInformation.Key,
                Server = longPollServerInformation.Server,
                Ts = longPollServerInformation.Ts.ToString()
            }).ContinueWith(_ =>
            {
                longPollServerInformation.Update(Convert.ToUInt64(_.Result.Ts));

                return _.Result;
            }, cancellationToken);
        }

        protected override IEnumerable<GroupUpdate> ConvertLongPollResponse(
            BotsLongPollHistoryResponse longPollResponse)
        {
            foreach (GroupUpdate groupUpdate in longPollResponse.Updates)
            {
                if (Configuration.AllowedUpdateTypes != null &&
                    Array.IndexOf(Configuration.AllowedUpdateTypes, groupUpdate.Type) == -1)
                    continue;

                yield return groupUpdate;
            }
        }
    }
}
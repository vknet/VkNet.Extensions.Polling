using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using VkNet.Abstractions;
using VkNet.Extensions.Polling.Models.Configuration;
using VkNet.Extensions.Polling.Models.State;

namespace VkNet.Extensions.Polling
{
    public abstract class LongPollBase<TLongPollResponse, TLongPollUpdate, TLongPollServerState, TLongPollConfiguration>
        where TLongPollServerState : ILongPollServerState
        where TLongPollConfiguration : ILongPollConfiguration
    {
        private readonly CancellationTokenSource _longPollStopTokenSource;
        private readonly IVkApi _vkApi;

        private readonly ChannelWriter<TLongPollUpdate> _updateChannelWriter;
        private readonly ChannelReader<TLongPollUpdate> _updateChannelReader;

        protected LongPollBase(IVkApi vkApi)
        {
            var updateChannel = Channel.CreateUnbounded<TLongPollUpdate>(
                new UnboundedChannelOptions()
                {
                    SingleWriter = true
                });

            _updateChannelReader = updateChannel;
            _updateChannelWriter = updateChannel;

            _vkApi = vkApi;
            _longPollStopTokenSource = new CancellationTokenSource();

            if (!Validate(vkApi))
            {
                throw new NotSupportedException("Выбранный тип лонг пулла недоступен для данного аккаунта.");
            }
        }

        protected abstract bool Validate(IVkApi vkApi);

        public TLongPollConfiguration Configuration { get; private set; }

        public async Task Start(TLongPollConfiguration longPollConfiguration,
            CancellationToken cancellationToken = default)
        {
            Configuration = longPollConfiguration;
            var linkedTokenSource =
                CancellationTokenSource.CreateLinkedTokenSource(_longPollStopTokenSource.Token, cancellationToken);

            var longPollServerInformation =
                await GetServerInformationAsync(_vkApi, longPollConfiguration, linkedTokenSource.Token);

            await Task.Factory.StartNew(async () =>
            {
                while (!linkedTokenSource.IsCancellationRequested)
                {
                    bool needRepeat;

                    TLongPollResponse longPollResponse = default;

                    do
                    {
                        try
                        {
                            longPollResponse = await GetUpdatesAsync(_vkApi, longPollConfiguration,
                                longPollServerInformation,
                                linkedTokenSource.Token);

                            needRepeat = false;
                        }
                        catch
                        {
                            try
                            {
                                longPollServerInformation =
                                    await GetServerInformationAsync(_vkApi, longPollConfiguration,
                                        linkedTokenSource.Token);
                            }
                            catch
                            {
                            }

                            needRepeat = true;
                        }
                    } while (needRepeat);

                    var updates = ConvertLongPollResponse(longPollResponse);

                    foreach (var update in updates)
                    {
                        await _updateChannelWriter.WriteAsync(update, cancellationToken: linkedTokenSource.Token);
                    }

                    await Task.Delay(Configuration.RequestDelay, linkedTokenSource.Token);
                }
            }, linkedTokenSource.Token);
        }

        protected abstract Task<TLongPollServerState> GetServerInformationAsync(IVkApi vkApi,
            TLongPollConfiguration longPollConfiguration, CancellationToken cancellationToken = default);

        protected abstract Task<TLongPollResponse> GetUpdatesAsync(IVkApi vkApi,
            TLongPollConfiguration longPollConfiguration, TLongPollServerState longPollServerStatus,
            CancellationToken cancellationToken = default);

        protected abstract IEnumerable<TLongPollUpdate> ConvertLongPollResponse(TLongPollResponse longPollResponse);


        public Task Stop()
        {
            return Task.Factory.StartNew(_longPollStopTokenSource.Cancel);
        }

        public ChannelReader<TLongPollUpdate> AsChannelReader()
        {
            return _updateChannelReader;
        }
    }
}
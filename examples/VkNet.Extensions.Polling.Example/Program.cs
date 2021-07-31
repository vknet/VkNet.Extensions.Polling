using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using VkNet.Extensions.Polling.Models.Configuration;
using VkNet.Extensions.Polling.Models.Update;
using VkNet.Model;
using VkNet.Model.GroupUpdate;

namespace VkNet.Extensions.Polling.Example
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File("Logs/App.log")
                .WriteTo.Console(LogEventLevel.Debug)
                .CreateLogger();
            
            IServiceCollection serviceCollection = new ServiceCollection()
                .AddLogging(loggingBuilder =>
                {
                    loggingBuilder.ClearProviders();
                    loggingBuilder.SetMinimumLevel(LogLevel.Trace);
                    loggingBuilder.AddSerilog(dispose: true);
                });
            
            Log.Information("VkNet.Extensions.Polling. Тестовое приложение.");
            
            Console.Write("Введите токен доступа (можно как для группы, так и для пользователя): ");

            string accessToken = Console.ReadLine();

            VkApi vkApi = new VkApi(serviceCollection);
            
            vkApi.Authorize(new ApiAuthParams()
            {
                AccessToken = accessToken
            });

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            
            if (vkApi.IsAuthorizedAsUser())
            {
                UserLongPoll userLongPoll = vkApi.StartUserLongPollAsync(UserLongPollConfiguration.Default, cancellationTokenSource.Token);

                StartReceiving(userLongPoll.AsChannelReader(), PrintUserUpdate);
            }

            else if(vkApi.IsAuthorizedAsGroup())
            {
                GroupLongPoll groupLongPoll = vkApi.StartGroupLongPollAsync(GroupLongPollConfiguration.Default, cancellationTokenSource.Token);

                StartReceiving(groupLongPoll.AsChannelReader(), PrintGroupUpdate);
            }
            else
            {
                Log.Information("Для корректной работы приложения требуется авторизация.");
            }

            while (true)  
            {
                Log.Information("Нажмите E чтобы прервать выполнение приложения.");
                
                if (Console.ReadKey().Key == ConsoleKey.E)
                {
                    Log.Information("Вы нажали Е. Производится закрытие приложения.");
                    
                    cancellationTokenSource.Cancel();
                    break;
                }
            }
            
        }

        private static void PrintUserUpdate(UserUpdate userUpdate)
        {
            Log.ForContext("Update", userUpdate)
                .Information($"Получен пользовательский апдейт: {userUpdate.Message.Id}. Текст: {userUpdate.Message.Text}..");
        }
        
        private static void PrintGroupUpdate(GroupUpdate groupUpdate)
        {
            Log.ForContext("Update", groupUpdate)
                .Information($"Получен групповой апдейт для группы {groupUpdate.GroupId}. Тип: {groupUpdate.Type}.");
        }
        
        
        private static async Task StartReceiving<TUpdate>(ChannelReader<TUpdate> channelReader, Action<TUpdate> updateAction)
        {
            IAsyncEnumerable<TUpdate> updateAsyncEnumerable = channelReader.ReadAllAsync();

            await foreach (TUpdate update in updateAsyncEnumerable)
            {
                updateAction(update);
            }
        }
    }
}
using Discord.WebSocket;

using discordbot.DAL.Entities;
using discordbot.DAL.Interfaces;
using discordbot.Services;
using discordbot.Services.DTOs;

using Google.Apis.YouTube.v3;

using LiteDB;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace discordbot.BackgroundServices
{
    class KonluluStreamGrabHostedService : BackgroundService
    {
        private readonly IBackgroundTaskQueue<int> taskQueue;
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly ILogger<KonluluStreamGrabHostedService> logger;
        private readonly YoutubeInterface ytInterface;

        public KonluluStreamGrabHostedService(IBackgroundTaskQueue<int> taskQueue, IServiceScopeFactory serviceScopeFactory, ILogger<KonluluStreamGrabHostedService> logger, YoutubeInterface ytInterface)
        {
            this.taskQueue = taskQueue;
            this.serviceScopeFactory = serviceScopeFactory;
            this.logger = logger;
            this.ytInterface = ytInterface;
        }
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Timer Manager Service started");
            taskQueue.QueueBackgroundWorkItem((c) => KonluluStreamGrabHostedService.RecurringTimer(c, 1));
            return base.StartAsync(cancellationToken);
        }

        public static Task<int> RecurringTimer(CancellationToken cancellationToken, int time)
        {
            return Task.Delay(time).ContinueWith((c) =>
            {
                return 0;
            });
        }

        protected override async Task ExecuteAsync(CancellationToken cancelToken)
        {
            try
            {
                while (!cancelToken.IsCancellationRequested)
                {
                    try
                    {
                        Func<CancellationToken, Task<int>> workItem = await taskQueue.DequeueAsync(cancelToken);
                        int stuff = await workItem(cancelToken);
                        {
                            using (IServiceScope scope = serviceScopeFactory.CreateScope())
                            {
                                IVideoRepository videoDb = scope.ServiceProvider.GetRequiredService<IVideoRepository>();

                                //todo replace with a mechanism of auto stream grabbing
                                YouTubeService ytService = ytInterface.GetYoutubeService();
                                string livestreamId = await ytInterface.GetLiveStream(ytService, "Live");
                                if (livestreamId == null)
                                {

                                    logger.LogInformation("found no upcoming stream");
                                }
                                VideoDto video = await ytInterface.GetVideoInfo(ytService, livestreamId);
                                videoDb.Save(new Video(video));
                                logger.LogInformation($"captured {livestreamId}");

                                taskQueue.QueueBackgroundWorkItem((c) => KonluluStreamGrabHostedService.RecurringTimer(c, 60));

                                //IGameRepository gameDb = scope.ServiceProvider.GetRequiredService<IGameRepository>();
                                //ObjectId gameId = new ObjectId(konluluTimer.gameId);
                                //GameEntity game = gameDb.Get(gameId);
                                //game.FuseCount++;
                                //logger.LogInformation(DateTime.Now.ToShortTimeString() + $" fuse:{konluluTimer.gameId}:{game.FuseCount}");

                                //if (game.FuseCount * 1000 < game.FuseTime)
                                //{
                                //    gameDb.Save(game);
                                //    taskQueue.QueueBackgroundWorkItem((c) => KonluluModule.FuseTimer(c, game));
                                //}
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine(ex.Message);
                        sb.AppendLine(ex.StackTrace);
                        logger.LogError(sb.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Exception when loading command modules");
                sb.AppendLine(ex.Message);
                sb.AppendLine(ex.StackTrace);
                logger.LogError(sb.ToString());
            }
        }
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Fuse Timer Manager Service stopped");
            return base.StopAsync(cancellationToken);
        }
    }
}

using Discord.WebSocket;

using discordbot.DAL.Entities;
using discordbot.DAL.Interfaces;
using discordbot.Services;
using discordbot.Services.DTOs;
using discordbot.Services.Interfaces;

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
        private readonly IBackgroundTaskQueue<KonluluStreamGrabStateObject> taskQueue;
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly ILogger<KonluluStreamGrabHostedService> logger;
        private readonly YoutubeInterface ytInterface;

        public KonluluStreamGrabHostedService(IBackgroundTaskQueue<KonluluStreamGrabStateObject> taskQueue, IServiceScopeFactory serviceScopeFactory, ILogger<KonluluStreamGrabHostedService> logger, YoutubeInterface ytInterface)
        {
            this.taskQueue = taskQueue;
            this.serviceScopeFactory = serviceScopeFactory;
            this.logger = logger;
            this.ytInterface = ytInterface;
        }
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Timer Manager Service started");
            taskQueue.QueueBackgroundWorkItem((c) =>
                                                    StateTimer(c, new KonluluStreamGrabStateObject(
                                                        KonluluStreamGrabState.GetUpcomingStream,
                                                        DateTime.UtcNow,
                                                        DateTime.UtcNow.AddSeconds(1))
                                                    )
                                                );
            return base.StartAsync(cancellationToken);
        }

        /*
         * automatically getting the livestream
         * there are several state:
         * - get upcoming stream
         *      perpetually call ytservice for upcoming stream search request, interval = 3600
         *      when get one, transit to next state
         * - wait for the upcoming stream
         *      wait till the upcoming stream's planned start time
         *      after finished waiting, transit to next state
         * - wait for actual start of stream
         *      perpetually call ytservice for stream's ActualStartTime, interval = 120
         *      when ActualStartTime is not null, store it in db, transit to next state
         * - wait for end of stream
         *      perpetually call ytservice for stream's ActualEndTime, interval = 900
         *      
         * youtube quota for scenarior: 1 day, 1 stream, stream last 4 hours, delay start time is 10 min
         * - get upcoming stream
         *      search request count: 18 
         *      quota:  18*100
         * - wait for actual start of stream
         *      video list request count: 5
         *      quota: 5*1
         * - wait for end of stream
         *      video list request count: 16
         *      quota: 16*1
         * + sum: 1822
         *      
         * youtube quota for scenarior: 1 day, 1 stream, stream last 10 hours, delay start time is 20 min
         * - get upcoming stream
         *      search request count: 18 
         *      quota:  14*100
         * - wait for actual start of stream
         *      video list request count: 10
         *      quota: 10*1
         * - wait for end of stream
         *      video list request count: 40
         *      quota: 40*1
         * + sum: 1451
         *      
         * youtube quota for scenarior: 1 day, 0 stream
         * - get upcoming stream
         *      search request count: 24
         *      quota:  24*100
         * + sum: 2400
         */

        internal class KonluluStreamGrabStateObject
        {
            public KonluluStreamGrabStateObject(KonluluStreamGrabState state, DateTime stateStart, DateTime stateWaitUntil)
            {
                this.State = state;
                this.StateStart = stateStart;
                this.StateWaitUntil = stateWaitUntil;
            }

            public KonluluStreamGrabState State { get; private set; }
            public DateTime StateStart { get; private set; }
            public DateTime StateWaitUntil { get; private set; }
            public string VideoId { get; set; } = null;
        }

        internal enum KonluluStreamGrabState
        {
            GetUpcomingStream,
            WaitForTheUpcomingStream,
            WaitForActualStartOfStream,
            WaitForEndOfStream
        }

        private static Task<KonluluStreamGrabStateObject> StateTimer(CancellationToken cancellationToken, KonluluStreamGrabStateObject stateobj)
        {
            int time = (int)(stateobj.StateWaitUntil - DateTime.UtcNow).TotalMilliseconds;
            if (time < 0)
            {
                time = 0;
            }
            return Task.Delay(time).ContinueWith((c) =>
                   {
                       return stateobj;
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
                        Func<CancellationToken, Task<KonluluStreamGrabStateObject>> workItem = await taskQueue.DequeueAsync(cancelToken);
                        KonluluStreamGrabStateObject stateObj = await workItem(cancelToken);
                        {
                            switch (stateObj.State)
                            {
                                case KonluluStreamGrabState.GetUpcomingStream:
                                    {
                                        YouTubeService ytService = ytInterface.GetYoutubeService();
                                        string livestreamId = await ytInterface.GetLiveStream(ytService, "Upcoming");
                                        bool foundLivestream = livestreamId != null;
                                        if (livestreamId == null)
                                        {
                                            logger.LogInformation("found no upcoming stream");
                                            livestreamId = await ytInterface.GetLiveStream(ytService, "Live");
                                            foundLivestream = livestreamId != null;
                                            if (livestreamId == null)
                                            {
                                                logger.LogInformation("found no live stream");
                                            }
                                        }

                                        if (!foundLivestream)
                                        {
                                            taskQueue.QueueBackgroundWorkItem((c) =>
                                                    StateTimer(c, new KonluluStreamGrabStateObject(
                                                        KonluluStreamGrabState.GetUpcomingStream,
                                                        DateTime.UtcNow,
                                                        DateTime.UtcNow.AddMinutes(30))
                                                    )
                                                );
                                        }
                                        else
                                        {
                                            logger.LogInformation($"found upcoming stream {livestreamId}");
                                            VideoDto video = await ytInterface.GetVideoInfo(ytService, livestreamId, true);

                                            taskQueue.QueueBackgroundWorkItem((c) =>
                                                    StateTimer(c, new KonluluStreamGrabStateObject(
                                                        KonluluStreamGrabState.WaitForTheUpcomingStream,
                                                        DateTime.UtcNow,
                                                        video.StartTime)
                                                    {
                                                        VideoId = livestreamId
                                                    }
                                                    )
                                                );
                                        }
                                        break;
                                    }
                                case KonluluStreamGrabState.WaitForTheUpcomingStream:
                                    {
                                        taskQueue.QueueBackgroundWorkItem((c) =>
                                                    StateTimer(c, new KonluluStreamGrabStateObject(
                                                        KonluluStreamGrabState.WaitForActualStartOfStream,
                                                        DateTime.UtcNow,
                                                        DateTime.UtcNow.AddSeconds(1))
                                                    {
                                                        VideoId = stateObj.VideoId
                                                    }
                                                    )
                                                );
                                        break;
                                    }
                                case KonluluStreamGrabState.WaitForActualStartOfStream:
                                    {
                                        YouTubeService ytService = ytInterface.GetYoutubeService();
                                        VideoDto video = null;
                                        try
                                        {
                                            video = await ytInterface.GetVideoInfo(ytService, stateObj.VideoId);
                                        }
                                        catch (StartCapturingTooSoonException)
                                        {
                                            taskQueue.QueueBackgroundWorkItem((c) =>
                                                        StateTimer(c, new KonluluStreamGrabStateObject(
                                                            KonluluStreamGrabState.WaitForActualStartOfStream,
                                                            DateTime.UtcNow,
                                                            DateTime.UtcNow.AddMinutes(2))
                                                        {
                                                            VideoId = stateObj.VideoId
                                                        }
                                                        )
                                                    );
                                            break;
                                        }
                                        catch (Exception ex)
                                        {
                                            logger.LogError(ex, nameof(KonluluStreamGrabState.WaitForActualStartOfStream));
                                            break;
                                        }

                                        using (IServiceScope scope = serviceScopeFactory.CreateScope())
                                        {
                                            IVideoRepository videoDb = scope.ServiceProvider.GetRequiredService<IVideoRepository>();
                                            ITagService tagService = scope.ServiceProvider.GetRequiredService<ITagService>();

                                            videoDb.Save(new Video(video));
                                            logger.LogInformation("captured {0}", video.VideoId);

                                            await tagService.StartTag(video.VideoId);
                                        }

                                        taskQueue.QueueBackgroundWorkItem((c) =>
                                                    StateTimer(c, new KonluluStreamGrabStateObject(
                                                        KonluluStreamGrabState.WaitForEndOfStream,
                                                        DateTime.UtcNow,
                                                        DateTime.UtcNow.AddSeconds(1))
                                                    {
                                                        VideoId = stateObj.VideoId
                                                    }
                                                    )
                                                );

                                        break;
                                    }
                                case KonluluStreamGrabState.WaitForEndOfStream:
                                    {
                                        YouTubeService ytService = ytInterface.GetYoutubeService();
                                        VideoDto video = null;
                                        try
                                        {
                                            video = await ytInterface.GetVideoInfo(ytService, stateObj.VideoId, getActualEndTime: true);
                                        }
                                        catch (NoActualEndtimeException)
                                        {
                                            taskQueue.QueueBackgroundWorkItem((c) =>
                                                    StateTimer(c, new KonluluStreamGrabStateObject(
                                                        KonluluStreamGrabState.WaitForEndOfStream,
                                                        DateTime.UtcNow,
                                                        DateTime.UtcNow.AddMinutes(15))
                                                    {
                                                        VideoId = stateObj.VideoId
                                                    }
                                                    )
                                                );
                                            break;
                                        }
                                        catch (Exception ex)
                                        {
                                            logger.LogError(ex, nameof(KonluluStreamGrabState.WaitForActualStartOfStream));
                                            break;
                                        }

                                        using (IServiceScope scope = serviceScopeFactory.CreateScope())
                                        {
                                            IVideoRepository videoDb = scope.ServiceProvider.GetRequiredService<IVideoRepository>();

                                            videoDb.Save(new Video(video));
                                            logger.LogInformation("waiting {0}", video.VideoId);
                                        }

                                        //taskQueue.QueueBackgroundWorkItem((c) =>
                                        //            StateTimer(c, new KonluluStreamGrabStateObject(
                                        //                KonluluStreamGrabState.GetUpcomingStream,
                                        //                DateTime.UtcNow,
                                        //                DateTime.UtcNow.AddSeconds(1))
                                        //            {
                                        //                VideoId = stateObj.VideoId
                                        //            }
                                        //            )
                                        //        );

                                        break;
                                    }
                                default:
                                    logger.LogError("you shouldnt be here");
                                    break;
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
            logger.LogInformation("Timer Manager Service stopped");
            return base.StopAsync(cancellationToken);
        }
    }
}

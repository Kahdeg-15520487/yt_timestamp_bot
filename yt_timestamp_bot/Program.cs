using Discord.Commands;
using Discord.WebSocket;
using discordbot.BackgroundServices;
using discordbot.DAL.Entities;
using discordbot.DAL.Implementations;
using discordbot.DAL.Interfaces;
using discordbot.Services;
using discordbot.Services.Interfaces;

using LiteDB;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using discordbot.DAL;
using discordbot.Services.Implementations;

namespace discordbot
{
    /*
     * this bot need 3 environment variables
     * yt_ts_BOTTOKEN
     * yt_ts_CONNSTR
     * yt_ts_APIKEY
     * yt_ts_PREFIX
     * yt_ts_LOG
     */

    class Program
    {
        public static readonly string APPLICATION_NAME = "yt_ts";
        public static readonly string VERSION = "0.5.0";

        public static void Main(string[] args)
        {
            try
            {
                new Program().MainAsync(args).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        public async Task MainAsync(string[] args)
        {
            await Host.CreateDefaultBuilder(args)
                      .ConfigureAppConfiguration((hostingContext, builder) =>
                      {
                          builder.AddJsonFile($"appsettings.json", true, true);
                          builder.AddEnvironmentVariables(prefix: APPLICATION_NAME);
                      })
                      .ConfigureServices(ConfigureServices)
                      .ConfigureLogging((hostContext, builder) =>
                      {
                          builder.ClearProviders();
                          builder.AddConsole();
                          builder.AddFile(Path.Combine(hostContext.Configuration["_LOG"], APPLICATION_NAME + "-{Date}.txt"));
                      })
                      .ConfigureWebHostDefaults(wb =>
                      {
                          wb.UseStartup<Startup>()
                            .UseKestrel(opts =>
                              {
                                  opts.ListenAnyIP(5000);
                              });
                      })
                      .RunConsoleAsync();
        }

        private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            IConfiguration configuration = context.Configuration;

            services.AddHttpClient()
                    .AddSingleton<DiscordSocketClient>()
                    .AddSingleton<CommandService>()
                    .AddSingleton<CommandHandler>()

                    .AddSingleton<LiteDBContextFactory, LiteDBContextFactory>()
                    .Configure<LiteDbConfig>(options => options.ConnectionString = configuration["_CONNSTR"])
                    .AddTransient(typeof(IBaseRepository<>), typeof(BaseRepository<>))
                    .AddTransient<ITimeStampRepository, TimestampRepository>()
                    .AddTransient<IVideoRepository, VideoRepository>()
                    .AddTransient<ITagService, TagService>()
                    .AddTransient<IVideoService, VideoService>()
                    .AddTransient<IVideoRepository, VideoRepository>()
                    .AddTransient<YoutubeInterface>()

                    .AddSingleton(typeof(IBackgroundTaskQueue<>), typeof(BackgroundTaskQueue<>))
                    .AddHostedService<DiscordHandlerHostedService>()
                    .AddHostedService<KonluluStreamGrabHostedService>()
                    ;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Loaded service: ");
            foreach (ServiceDescriptor service in services)
            {
                sb.AppendLine($"Service: {service.ServiceType.FullName}\n      Lifetime: {service.Lifetime}\n      Instance: {service.ImplementationType?.FullName}");
            }

            ConfigureDatabase();
        }

        private static void ConfigureDatabase()
        {
            BsonMapper mapper = BsonMapper.Global;

            mapper.Entity<Video>()
                  .Id(x => x.VideoId);
        }
    }
}

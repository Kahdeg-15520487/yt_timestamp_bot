using Discord.Commands;
using Discord.WebSocket;

using discordbot;
using discordbot.BackgroundServices;
using discordbot.DAL.Entities;
using discordbot.DAL.Implementations;
using discordbot.DAL.Interfaces;
using discordbot.Services;
using discordbot.Services.Interfaces;

using discordbot.BackgroundServices;

using LiteDB;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System;
using System.Text;
using System.Threading.Tasks;

namespace discordbot
{
    /*
     * this bot need 3 environment variables
     * yt_ts_BOTTOKEN
     * yt_ts_CONNSTR
     * yt_ts_APIKEY
     * yt_ts_PREFIX
     */

    class Program
    {
        public static readonly string APPLICATION_NAME = "yt_ts";
        public static readonly string VERSION = "0.3.0";

        public static void Main(string[] args)
        {
            try
            {
                new Program().MainAsync(args).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {

            }
        }

        public async Task MainAsync(string[] args)
        {
            await Host.CreateDefaultBuilder(args)
                      .ConfigureServices(ConfigureServices)
                      .ConfigureLogging((hostContext, builder) =>
                      {
                          builder.ClearProviders();
                          builder.AddConsole();
                          builder.AddFile(APPLICATION_NAME + "-{Date}.txt");
                      })
                      .ConfigureWebHostDefaults(wb =>
                      {
                          wb.UseStartup<Startup>();
                      })
                      .RunConsoleAsync();

            //await new HostBuilder()
            //          .ConfigureServices(ConfigureServices)
            //          .ConfigureLogging((hostContext, builder) =>
            //          {
            //              builder.ClearProviders();
            //              builder.AddConsole();
            //              builder.AddFile(APPLICATION_NAME + "-{Date}.txt");
            //          })
            //          .RunConsoleAsync();
        }

        private static IConfiguration configuration;

        private static IConfiguration LoadConfiguration()
        {
            string env = Environment.GetEnvironmentVariable(APPLICATION_NAME + "_ENVIRONMENT");

            IConfigurationBuilder builder = new ConfigurationBuilder()
                              .AddJsonFile($"appsettings.json", true, true)
                              .AddEnvironmentVariables(APPLICATION_NAME);
            return builder.Build();
        }

        private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            configuration = LoadConfiguration();

            services.AddSingleton<IConfiguration>(configuration)
                    .AddHttpClient()
                    .AddSingleton<DiscordSocketClient>()
                    .AddSingleton<CommandService>()
                    .AddSingleton<CommandHandler>()

                    .AddSingleton<ILiteDatabase>(new LiteDatabase(configuration["_CONNSTR"]))
                    .AddTransient(typeof(IBaseRepository<>), typeof(BaseRepository<>))
                    .AddTransient<ITimeStampRepository, TimestampRepository>()
                    .AddTransient<IVideoRepository, VideoRepository>()
                    .AddTransient<ITagService, TagService>()
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

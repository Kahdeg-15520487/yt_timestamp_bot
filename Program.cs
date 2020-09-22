using Discord.Commands;
using Discord.WebSocket;

using DiscordBot.BackgroundServices;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot
{
    class Program
    {

        public static readonly string APPLICATION_NAME = "discord_bot";

        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            await new HostBuilder()
                      .ConfigureServices(ConfigureServices)
                      .ConfigureLogging((hostContext, builder) =>
                      {
                          builder.ClearProviders();
                          builder.AddConsole();
                          builder.AddFile(APPLICATION_NAME + "-{Date}.txt");
                      })
                      .RunConsoleAsync();
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

                    .AddHostedService<DiscordHandlerHostedService>()
                    ;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Loaded service: ");
            foreach (ServiceDescriptor service in services)
            {
                sb.AppendLine($"Service: {service.ServiceType.FullName}\n      Lifetime: {service.Lifetime}\n      Instance: {service.ImplementationType?.FullName}");
            }
        }
    }
}

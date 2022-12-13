using Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Repositories;
using Services;
using System;
using System.Threading.Tasks;

namespace DojoScraper
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            using (var serviceScope = host.Services.CreateScope())
            {
                var services = serviceScope.ServiceProvider;

                try
                {
                    var app = services.GetRequiredService<MyApp>();
                    return await app.RunAsync(args);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();

                    logger.LogError(ex, "An error occurred.");
                }
            }
            
            Console.ReadLine();

            return 0;
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    var configuration = hostContext.Configuration;

                    services.AddHttpClient<DojoRepository>()
                            .ConfigureHttpClient(c =>
                            {
                                c.BaseAddress = new Uri(configuration["dojoApi"]);
                                c.DefaultRequestHeaders.Add("Accept", "application/json");
                                c.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/78.0.3904.87 Safari/537.36");
                            });

                    services.AddSingleton<ICommandHandlerBuilder, DownloadImagesCommandHandlerBuilder>();
                    services.AddSingleton<MyApp>();
                    services.AddSingleton<AssetsManager>();
                });
    }
}

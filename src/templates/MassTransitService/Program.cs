using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using MassTransitService.Messages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MassTransitService
{
    public class MassTransitOptions
    {
#if (RabbitMQTransport)

        public string Host { get; set; }
        public string VirtualHost { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
#endif
    }

    public class Program
    {
        static async Task Main(string[] args)
        {
            var isService = !(Debugger.IsAttached || args.Contains("--console"));

            var builder = new HostBuilder()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: true);
                    config.AddEnvironmentVariables();

                    if (args != null)
                        config.AddCommandLine(args);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.Configure<MassTransitOptions>(hostContext.Configuration.GetSection("MassTransit"));

                    services.AddMassTransit(cfg =>
                    {
                        cfg.AddConsumer<TimeConsumer>();
                        cfg.AddBus(ConfigureBus);
                        cfg.AddRequestClient<IsItTime>();
                    });

                    services.AddSingleton<IHostedService, MassTransitConsoleHostedService>();
                    services.AddSingleton<IHostedService, CheckTheTimeService>();
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                });

            if (isService)
            {
                await builder.RunAsServiceAsync();
            }
            else
            {
                await builder.RunConsoleAsync();
            }
        }
#if (RabbitMQTransport)
        static IBusControl ConfigureBus(IServiceProvider provider)
        {
            var massTransitOptions = provider.GetRequiredService<IOptions<MassTransitOptions>>().Value;

            return Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                var host = cfg.Host(massTransitOptions.Host, massTransitOptions.VirtualHost, h =>
                {
                    h.Username(massTransitOptions.Username);
                    h.Password(massTransitOptions.Password);
                });

                cfg.ConfigureEndpoints(provider);
            });
        }
#else
        static IBusControl ConfigureBus(IServiceProvider provider)
        {
            return Bus.Factory.CreateUsingInMemory(cfg =>
            {
                cfg.ConfigureEndpoints(provider);
            });
        }
#endif

    }
}

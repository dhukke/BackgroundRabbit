using BackgroundRabbit.Database;
using BackgroundRabbit.Handlers;
using BackgroundRabbit.MessageBroker;
using BackgroundRabbit.Workers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace BackgroundRabbit
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog((context, configuration) =>
                    {
                        configuration
                            .Enrich.FromLogContext()
                            .Enrich.WithMachineName()
                            .WriteTo.Console()
                            .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
                            .ReadFrom.Configuration(context.Configuration);

                    }
                )
                .ConfigureServices((hostContext, services) =>
                    {
                        services.AddHostedService<ConsumerWorker>();
                        services.AddHostedService<PublisherWorker>();
                        services.AddSingleton<IProducer, Producer>();
                        services.AddScoped<ConsumerHandler>();
                        services.AddDbContext<MessagesContext>(options =>
                            options.UseSqlServer(
                                "Server=localhost,1433;User ID=sa;Password=yourStrong(!)Password;Initial Catalog=messagedb;"
                            ),
                            ServiceLifetime.Scoped
                        );
                    }
                );
    }
}

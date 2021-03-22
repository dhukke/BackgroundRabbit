using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
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
                        services.AddScoped(_  =>  new  ConnectionFactory{HostName = "localhost"});
                        services.AddScoped<ConsumerHandler>();
                    }
                );
    }
}

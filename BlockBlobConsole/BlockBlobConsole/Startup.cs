using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BlockBlobConsole
{
    public static class Startup
    {
        public static IServiceCollection ConfigureServices()
        {
            string env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (string.IsNullOrWhiteSpace(env))
            {
                throw new Exception("ASPNETCORE_ENVIRONMENT env variable not set.");
            }

            Console.WriteLine($"Bootstrapping application using environment {env}");

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"appsettings.{env}.json", optional: false, reloadOnChange: false)
                .AddEnvironmentVariables();

            IConfiguration configuration = builder.Build();

            var services = new ServiceCollection()
                .AddLogging(logging => logging.Services.AddLogging());

            services.AddOptions();

            var configurationSection = configuration.GetSection(nameof(BlobStorageConfig));

            services.Configure<BlobStorageConfig>(options => configurationSection.Bind(options));
            services.AddSingleton<ILoggerFactory>(new LoggerFactory());
            services.AddSingleton(configuration);
            services.AddTransient<IBlockBlobClientFactory, BlockBlobClientFactory>();
            services.AddTransient<IBlockBlobRepository, BlockBlobRepository>();
            services.AddTransient<ConsoleApplication>();

            services.AddSingleton<IServiceProvider>(services.BuildServiceProvider());

            return services;
        }
    }
}
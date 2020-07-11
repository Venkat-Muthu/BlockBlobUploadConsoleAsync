using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace BlockBlobConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var services = Startup.ConfigureServices();

            string relativePath = ".\\content\\";
            var provider = services.BuildServiceProvider();
            var files = Directory.GetFiles(RelativePath, "*.*");
            var containerName = $"{DateTime.UtcNow:yyyy-MM}";

            foreach (var file in files)
            {
                provider.GetService<ConsoleApplication>().Run(containerName, relativePath, Path.GetFileName(file));
            }

            Console.ReadLine();
        }
    }
}

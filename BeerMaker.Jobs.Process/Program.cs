using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using BeerMaker.Core.Models.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Topshelf.Runtime.DotNetCore;
using Unosquare.RaspberryIO;
using Unosquare.WiringPi;

namespace BeerMaker.Jobs.Process
{
    class Program
    {
        
        static void Main(string[] args)
        {
            var builder = new HostBuilder()
                .ConfigureHostConfiguration(configHost => { configHost.SetBasePath(Directory.GetCurrentDirectory()); })
                .ConfigureAppConfiguration((hostContext, configApp) =>
                {
                    configApp.AddJsonFile("ConfigBeerMakerJobProcess.json", true);
                    configApp.AddEnvironmentVariables();

                })
                .ConfigureServices((hostContext, services) =>
                {
                    var settings = hostContext.Configuration.Get<BearMakerSettings>();

                    
                    services.AddSingleton(settings);
                    services.AddLogging();
                    //if (settings.UsePostreSQL)
                    //{
                    //    services.AddScoped<ISendEmailRepository, PostgreSQL.SendEmailRepository>();
                    //}
                    //else
                    //{
                    //    services.AddScoped<ISendEmailRepository, MSSQL.SendEmailRepository>();
                    //}

                    Pi.Init<BootstrapWiringPi>();
                    services.AddHostedService<BeerMakerService>();
                })
                .ConfigureLogging((hostContext, configLogging) =>
                {
                    configLogging.AddConsole();
                    configLogging.AddDebug();
                    configLogging.SetMinimumLevel(LogLevel.Debug);
                });

            builder.RunAsTopshelfService(hc =>
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    hc.UseEnvironmentBuilder(c => new DotNetCoreEnvironmentBuilder(c));
                }
                hc.SetServiceName("BeerMaker.Jobs.Process");
                hc.SetDisplayName("BeerMaker Main Process");
                hc.SetDescription("Detecting temprature and starting hot and cold beer");
            });
        }
    }
}

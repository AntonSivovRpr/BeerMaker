using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Topshelf;

namespace BeerMaker.Jobs.Process
{
    public static class HostBuilderExtentions
    {
        public static IHostBuilder UseTopshelfLifetime(this IHostBuilder hostBuilder)
        {
            return hostBuilder.ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<IHostLifetime, TopshelfLifetime>();
            });
        }

        public static TopshelfExitCode RunAsTopshelfService(this IHostBuilder hostBuilder, Action<Topshelf.HostConfigurators.HostConfigurator> configureTopshelfHost)
        {
            if (configureTopshelfHost == null) throw new ArgumentNullException(nameof(configureTopshelfHost));

            hostBuilder.UseTopshelfLifetime();

            var rc = HostFactory.Run(x =>
            {
                configureTopshelfHost(x);
                x.Service<IHost>((Action<Topshelf.ServiceConfigurators.ServiceConfigurator<IHost>>)(s =>
                {
                    s.ConstructUsing(() => hostBuilder.Build());
                    s.WhenStarted(service =>
                    {
                        service.Start();
                    });
                    s.WhenStopped(service =>
                    {
                        service.StopAsync();
                    });
                }));
            });

            return rc;

        }
    }
}

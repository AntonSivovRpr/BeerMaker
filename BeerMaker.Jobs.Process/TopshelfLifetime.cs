using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace BeerMaker.Jobs.Process
{
    public class TopshelfLifetime : IHostLifetime
    {

        public TopshelfLifetime(IHostApplicationLifetime applicationLifetime, IServiceProvider services)
        {
            ApplicationLifetime = applicationLifetime ?? throw new ArgumentNullException(nameof(applicationLifetime));
        }

        private IHostApplicationLifetime ApplicationLifetime { get; }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task WaitForStartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
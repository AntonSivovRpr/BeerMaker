using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BeerMaker.Core.Models.Settings;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BeerMaker.Jobs.Process
{
    public class BeerMakerService : IHostedService
    {

        private readonly ILogger _log;
        private readonly BearMakerSettings _settings;

        //private readonly IBeerMakerRepository _beerMakerRepository;

        public BeerMakerService(
            BearMakerSettings settings,
            ILogger<BeerMakerService> log)
        {
            _log = log;
            _settings = settings;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await ScheduledWork.Start(_settings, _log);
        }


        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}

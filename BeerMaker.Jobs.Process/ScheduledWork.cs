using System;
using System.IO;
using System.Threading.Tasks;
using BeerMaker.Core.Models.Settings;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Impl;

namespace BeerMaker.Jobs.Process
{
    public class ScheduledWork
    {
        
        public static async Task Start(BearMakerSettings settings, ILogger logger)
        {
            var scheduler = await StdSchedulerFactory.GetDefaultScheduler();

            await scheduler.Start();

            var job = JobBuilder.Create<BeerMakerCoreJob>().Build();

            job.JobDataMap.Put("settings", settings);
            //job.JobDataMap.Put("sendEmailRepository", sendEmailRepository);
            job.JobDataMap.Put("log", logger);


            var trigger = TriggerBuilder.Create()
                .WithIdentity("beerMakerJob", "BeerMakerJob")
                .StartNow()
                .WithSimpleSchedule(x => x.WithInterval(TimeSpan.FromSeconds(1)).RepeatForever())
                .Build();


            try
            {
                await scheduler.ScheduleJob(job, trigger);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
            }
        }


    }
}
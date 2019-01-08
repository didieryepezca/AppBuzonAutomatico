using Quartz;
using Quartz.Impl;
using System;

namespace ReadInboxEmail.WebApplication.Data
{
    public class JobScheduler
    {
        public static void Start()
        {
            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.Start();

            IJobDetail job = JobBuilder.Create<DA_MailBox>().Build();

            ITrigger trigger = TriggerBuilder.Create()
              .WithIdentity("trigger1", "group1")
              .StartNow()
              .WithSimpleSchedule(x => x
                  .WithIntervalInSeconds(5)
                  .RepeatForever())
              .Build();

            scheduler.ScheduleJob(job, trigger);
        }
    }
}
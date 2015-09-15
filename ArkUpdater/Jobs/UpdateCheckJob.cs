using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Quartz;
using SSQLib;

namespace ArkUpdater.Jobs
{
    [DisallowConcurrentExecution]
    class UpdateCheckJob : IJob
    {
        static readonly Regex Regex = new Regex("\"public\"\\s*{\\s*\"buildid\"\\s*\"(?<Revision>\\d{6,})\"", RegexOptions.Compiled);

        public void Execute(IJobExecutionContext context)
        {
            Log.LogToConsole("Checking for updates");

            if (CheckForUpdates())
            {
                Log.LogToConsole("Update found");

                context.Scheduler.PauseTrigger(context.Trigger.Key);

                ITrigger trigger;
                Quartz.Collection.ISet<ITrigger> triggers = new Quartz.Collection.HashSet<ITrigger>();
                switch (Settings.Instance.UpdateType)
                {
                    case 1:
                        context.Scheduler.TriggerJob(new JobKey("Update"));
                        break;
                    case 2:
                        context.Scheduler.TriggerJob(new JobKey("Broadcast"), new JobDataMap { { "Message", Settings.Instance.UpdateFoundMessage.Replace("%minutes%", Settings.Instance.WaitTime.ToString()) } });

                        if (Settings.Instance.WaitTime > 10)
                        {
                            trigger = TriggerBuilder.Create()
                                .StartAt(DateBuilder.FutureDate(Settings.Instance.WaitTime - 10, IntervalUnit.Minute))
                                .ForJob(context.Scheduler.GetJobDetail(new JobKey("Broadcast")))
                                .UsingJobData("Message", Settings.Instance.TenMinutesMessage.Replace("%minutes%", "10"))
                                .Build();
                            triggers.Add(trigger);
                        }

                        // Trigger for one Minute before
                        trigger = TriggerBuilder.Create()
                            .StartAt(DateBuilder.FutureDate(Settings.Instance.WaitTime - 1, IntervalUnit.Minute))
                            .ForJob(context.Scheduler.GetJobDetail(new JobKey("Broadcast")))
                            .UsingJobData("Message", Settings.Instance.OneMinuteMessage.Replace("%minutes%", "1"))
                            .Build();
                        triggers.Add(trigger);

                        context.Scheduler.ScheduleJob(context.Scheduler.GetJobDetail(new JobKey("Broadcast")), triggers, true);

                        trigger = TriggerBuilder.Create()
                            .StartAt(DateBuilder.FutureDate(Settings.Instance.WaitTime, IntervalUnit.Minute))
                            .ForJob(context.Scheduler.GetJobDetail(new JobKey("Update")))
                            .Build();

                        context.Scheduler.ScheduleJob(trigger);
                        break;
                    case 3:
                        context.Scheduler.TriggerJob(new JobKey("Broadcast"), new JobDataMap { { "Message", Settings.Instance.UpdateFoundMessage.Replace("%minutes%", Settings.Instance.WaitTime.ToString()) } });

                        trigger = TriggerBuilder.Create()
                            .WithSimpleSchedule(x => x
                                .WithIntervalInMinutes(5)
                                .RepeatForever())
                            .ForJob(context.Scheduler.GetJobDetail(new JobKey("CheckForPlayers")))
                            .StartNow()
                            .Build();

                        context.Scheduler.ScheduleJob(trigger);
                        break;
                }

            }
            else
                Log.LogToConsole("No update available");
        }

        private bool CheckForUpdates()
        {
            try
            {
                FileInfo fi = new FileInfo(Settings.Instance.SteamCmd);
                if (Directory.Exists(Path.Combine(fi.Directory.FullName, "appcache")))
                    Directory.Delete(Path.Combine(fi.Directory.FullName, "appcache"), true);


                string output;
                using (Process process = new Process())
                {
                    process.StartInfo.FileName = Settings.Instance.SteamCmd;
                    process.StartInfo.Arguments = $"+login anonymous +app_info_update 1 +app_info_print {Settings.Instance.ArkAppId} +app_info_print {Settings.Instance.ArkAppId} +quit";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.Start();
                    output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();
                }

                Match match = Regex.Match(output);
                if (match.Success)
                {
                    string revision = match.Groups["Revision"].Value;
                    long newRevision = long.Parse(revision);
                    Settings.Instance.LatestFoundRevision = newRevision;

                    return newRevision > Settings.Instance.InstalledRevision;
                }
            }
            catch (Exception ex)
            {
                Log.LogErrorToConsole("Update check failed");
                Log.LogErrorToConsole(ex.ToString());
            }

            return false;
        }
    }
}

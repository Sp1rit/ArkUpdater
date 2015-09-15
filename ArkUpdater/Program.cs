using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;

namespace ArkUpdater
{
    class Program
    {
        private static StringBuilder sb = new StringBuilder();

        static void Main(string[] args)
        {
            // Check Settings
            FileInfo fi = new FileInfo(Settings.Instance.SteamCmd);
            if (!fi.Exists || (fi.Name != "steamcmd.exe" && fi.Name != "steamcmd.sh"))
            {
                Log.LogErrorToConsole("SteamCmd path incorrect");
                Log.LogErrorToConsole("Press any key to exit");
                Console.ReadKey();
                return;
            }

            DirectoryInfo shooterGame = new DirectoryInfo(Path.Combine(Settings.Instance.ArkFolder, "ShooterGame"));
            DirectoryInfo engine = new DirectoryInfo(Path.Combine(Settings.Instance.ArkFolder, "Engine"));
            if (!shooterGame.Exists || !engine.Exists)
            {
                Log.LogErrorToConsole("ArkFolder path incorrect");
                Log.LogErrorToConsole("Press any key to exit");
                Console.ReadKey();
                return;
            }
            if (!Rcon.Instance.Connect(Settings.Instance.ServerHost, Settings.Instance.RconPort, Settings.Instance.RconPassword))
            //if (!Rcon.Instance.Connect(Settings.Instance.ServerHost, Settings.Instance.RconPort, Settings.Instance.RconPassword))
            {
                Log.LogErrorToConsole("Could not connect to the server using RCON");
                Log.LogErrorToConsole("Press any key to exit");
                Console.ReadKey();
                return;
            }
            //Rcon.Instance.Disconnect();
            Rcon.Instance.Disconnect();

            // Start updater
            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.Start();

            IJobDetail updateJob = JobBuilder.Create<Jobs.UpdateJob>()
                .WithIdentity("Update")
                .StoreDurably(true)
                .Build();

            scheduler.AddJob(updateJob, true);

            IJobDetail checkForPlayersJob = JobBuilder.Create<Jobs.CheckForPlayersJob>()
                .WithIdentity("CheckForPlayers")
                .StoreDurably(true)
                .Build();

            scheduler.AddJob(checkForPlayersJob, true);

            IJobDetail broadcastJob = JobBuilder.Create<Jobs.BroadcastJob>()
                .WithIdentity("Broadcast")
                .StoreDurably(true)
                .Build();

            scheduler.AddJob(broadcastJob, true);

            IJobDetail updateCheckJob = JobBuilder.Create<Jobs.UpdateCheckJob>()
                .WithIdentity("UpdateChecker")
                .UsingJobData(new JobDataMap { { "UpdateJobKey", updateJob.Key } })
                .Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("UpdateChecker")
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInMinutes(Settings.Instance.CheckInterval)
                    .RepeatForever())
                .ForJob(updateCheckJob)
                .Build();

            scheduler.ScheduleJob(updateCheckJob, trigger);

            while (Console.ReadLine() != "quit") { }

            scheduler.Shutdown();
        }

        private static void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            sb.AppendLine(e.Data);
        }
    }
}

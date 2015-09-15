using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quartz;

namespace ArkUpdater.Jobs
{
    [DisallowConcurrentExecution]
    class CheckForPlayersJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            Log.LogErrorToConsole("CheckForPlayersJob");
            // Check For players

            int playerCount = ServerChecker.GetPlayerCount(Settings.Instance.ServerHost, Settings.Instance.QueryPort);
            Log.LogToConsole("PlayerCount " + playerCount);
            if (playerCount == 0)
            {
                // Player count = 0
                context.Scheduler.UnscheduleJob(context.Trigger.Key);
                context.Scheduler.TriggerJob(new JobKey("Update"));
            }
        }
    }
}

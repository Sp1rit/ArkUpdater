using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;

namespace ArkUpdater.Jobs
{
    [DisallowConcurrentExecution]
    class BroadcastJob : IJob
    {
        public string Message { get; set; }

        public void Execute(IJobExecutionContext context)
        {
            Log.LogToConsole($"Broadcast: {Message}");

            if (!Rcon.Instance.IsConnected)
            {
                for(int i = 0; i < 3; i++)
                {
                    if (Rcon.Instance.Connect(Settings.Instance.ServerHost, Settings.Instance.RconPort, Settings.Instance.RconPassword))
                        break;
                }
            }

            if (Rcon.Instance.IsConnected)
            {
                Rcon.Instance.Broadcast(Message);
                Rcon.Instance.Disconnect();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Quartz;

namespace ArkUpdater.Jobs
{
    [DisallowConcurrentExecution]
    class UpdateJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            Log.LogErrorToConsole("Update started");

            try
            {
                bool goOn;

                if (!Rcon.Instance.IsConnected)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        if (Rcon.Instance.Connect(Settings.Instance.ServerHost, Settings.Instance.RconPort, Settings.Instance.RconPassword))
                            break;
                    }
                }

                if (!(goOn = Rcon.Instance.IsConnected))
                    Log.LogErrorToConsole("Rcon connection failed. Update aborted.");
                if (goOn && !(goOn = Rcon.Instance.ExitServer()))
                    Log.LogErrorToConsole("Could not stop the server. Update aborted.");
                if (goOn && !Update())
                    Log.LogErrorToConsole("Update failed.");
                if (goOn)
                    StartServer();
            }
            finally
            {
                context.Scheduler.ResumeTrigger(new TriggerKey("UpdateChecker"));
                Log.LogToConsole("UpdateChecker resumed.");
            }
        }

        private bool Update()
        {
            try
            {
                using (Process steamcmd = new Process())
                {
                    steamcmd.StartInfo.FileName = Settings.Instance.SteamCmd;
                    steamcmd.StartInfo.Arguments = $"+login anonymous +force_install_dir \"{Settings.Instance.ArkFolder}\" +app_update {Settings.Instance.ArkAppId} validate +quit";
                    steamcmd.StartInfo.UseShellExecute = false;
                    steamcmd.Start();
                    steamcmd.WaitForExit();
                }

                Settings.Instance.InstalledRevision = Settings.Instance.LatestFoundRevision;
                Settings.Instance.Save();

                return true;
            }
            catch (Exception ex)
            {
                Log.LogErrorToConsole(ex.ToString());
            }

            return false;
        }

        private bool StartServer()
        {
            try
            {
                using (Process game = new Process())
                {
                    game.StartInfo.FileName = Path.Combine(Settings.Instance.ArkFolder, "ShooterGame", "Binaries", "Win64", "ShooterGameServer.exe");
                    game.StartInfo.WorkingDirectory = Path.Combine(Settings.Instance.ArkFolder, "ShooterGame", "Binaries", "Win64");
                    game.StartInfo.Arguments = Settings.Instance.StartParameters;
                    game.StartInfo.UseShellExecute = false;
                    game.Start();
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.LogErrorToConsole(ex.ToString());
            }

            return false;
        }
    }
}

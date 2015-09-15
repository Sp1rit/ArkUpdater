//using SourceRcon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Rcon;
using Commands = Rcon.Commands;

namespace ArkUpdater
{
    class Rcon
    {
        #region Singleton

        private static readonly Lazy<Rcon> _instance = new Lazy<Rcon>(() => new Rcon());

        public static Rcon Instance
        {
            get { return _instance.Value; }
        }

        #endregion // Singleton

        private readonly RconClient client = new RconClient();

        public bool IsConnected => client.IsConnected;

        public bool Connect(string host, int port, string password)
        {
            try
            {
                client.Connect(host, port, password);

                return client.IsConnected;
            }
            catch (Exception ex)
            {
                Log.LogErrorToConsole(ex.Message);
            }

            return false;
        }

        public void Disconnect()
        {
            client?.Disconnect();
        }

        public bool ExitServer()
        {
            AutoResetEvent reset = new AutoResetEvent(false);
            bool success = false;
            client.ExecuteCommandAsync(new Commands.DoExit(), (s, e) =>
            {
                success = e.Successful;
                reset.Set();
            });
            reset.WaitOne();

            Disconnect();

            return success;
        }

        public bool Broadcast(string message)
        {
            AutoResetEvent reset = new AutoResetEvent(false);
            bool success = false;
            client.ExecuteCommandAsync(new Commands.Broadcast(message), (s, e) =>
            {
                success = e.Successful;
                reset.Set();
            });
            reset.WaitOne();

            return success;
        }

        public bool ServerChat(string message)
        {
            AutoResetEvent reset = new AutoResetEvent(false);
            bool success = false;
            client.ExecuteCommandAsync(new Commands.ServerChat(message), (s, e) =>
            {
                success = e.Successful;
                reset.Set();
            });
            reset.WaitOne();

            return success;
        }
    }
}

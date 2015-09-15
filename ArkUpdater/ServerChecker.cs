using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using SSQLib;

namespace ArkUpdater
{
    static class ServerChecker
    {
        private static SSQL ssql = new SSQL();
        private static Regex serverName = new Regex(@".* \(v(?<Version>[0-9.]+)\)", RegexOptions.Compiled);

        public static double GetServerVersion(string host, int port)
        {
            try
            {
                IPAddress ip = Dns.GetHostAddresses(host).First(i => i.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                ServerInfo server = ssql.Server(new IPEndPoint(ip, port));
                Match match = serverName.Match(server.Name);
                if (match.Success)
                {
                    double version = double.Parse(match.Groups["Version"].ToString(), CultureInfo.InvariantCulture);
                    return version;
                }
            }
            catch (Exception ex)
            {
                // Server down
            }

            return 0;
        }

        public static int GetPlayerCount(string host, int port)
        {
            try
            {
                IPAddress ip = Dns.GetHostAddresses(host).First(i => i.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                ServerInfo server = ssql.Server(new IPEndPoint(ip, port));
                return int.Parse(server.PlayerCount);
            }
            catch (Exception ex)
            {
                // Server down
            }

            return -1;
        }
    }
}

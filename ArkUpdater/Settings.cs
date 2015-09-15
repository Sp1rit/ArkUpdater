using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using MadMilkman.Ini;

namespace ArkUpdater
{
    class Settings
    {
        #region Singleton

        private static readonly Lazy<Settings> _instance = new Lazy<Settings>(() => new Settings());

        public static Settings Instance
        {
            get { return _instance.Value; }
        }

        #endregion // Singleton

        private static readonly string ApplicationPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        private static readonly string IniFile = Path.Combine(ApplicationPath, "ArkUpdater.ini");
        private readonly IniFile ini;

        public int CheckInterval
        {
            get
            {
                string sVal = ini.Sections["UpdateChecker"].Keys["CheckInterval"].Value;
                int iVal = string.IsNullOrEmpty(sVal) ? 10 : int.Parse(sVal);
                return (iVal >= 5 && iVal <= 60) ? iVal : 10;
            }
            set
            {
                if (value >= 5 && value <= 60)
                    ini.Sections["UpdateChecker"].Keys["CheckInterval"].Value = value.ToString();
            }
        }

        public string ServerHost
        {
            get { return ini.Sections["UpdateChecker"].Keys["ServerHost"].Value; }
            set { ini.Sections["UpdateChecker"].Keys["ServerHost"].Value = value; }
        }

        public int QueryPort
        {
            get
            {
                string sVal = ini.Sections["UpdateChecker"].Keys["QueryPort"].Value;
                int iVal = string.IsNullOrEmpty(sVal) ? 27015 : int.Parse(sVal);
                return (iVal >= 1 && iVal <= 65535) ? iVal : 27015;
            }
            set
            {
                if (value >= 1 && value <= 65535)
                    ini.Sections["UpdateChecker"].Keys["QueryPort"].Value = value.ToString();
            }
        }

        public int RconPort
        {
            get
            {
                string sVal = ini.Sections["UpdateChecker"].Keys["RconPort"].Value;
                int iVal = string.IsNullOrEmpty(sVal) ? 27020 : int.Parse(sVal);
                return (iVal >= 1 && iVal <= 65535) ? iVal : 27020;
            }
            set
            {
                if (value >= 1 && value <= 65535)
                    ini.Sections["UpdateChecker"].Keys["RconPort"].Value = value.ToString();
            }
        }

        public string RconPassword
        {
            get { return ini.Sections["UpdateChecker"].Keys["RconPassword"].Value; }
            set { ini.Sections["UpdateChecker"].Keys["RconPassword"].Value = value; }
        }

        public string ArkAppId
        {
            get { return ini.Sections["UpdateChecker"].Keys["ArkAppId"].Value; }
            set { ini.Sections["UpdateChecker"].Keys["ArkAppId"].Value = value; }
        }

        public long InstalledRevision
        {
            get
            {
                string sVal = ini.Sections["UpdateChecker"].Keys["InstalledRevision"].Value;
                long iVal = string.IsNullOrEmpty(sVal) ? 0 : long.Parse(sVal);
                return iVal;
            }
            set
            {
                if (value >= 0)
                    ini.Sections["UpdateChecker"].Keys["InstalledRevision"].Value = value.ToString();
            }
        }

        public long LatestFoundRevision { get; set; }

        public int UpdateType
        {
            get
            {
                string val = ini.Sections["Updater"].Keys["UpdateType"].Value;
                return (val == "1" || val == "2" || val == "3") ? int.Parse(val) : 2;
            }
            set
            {
                if (value >= 1 && value <= 3)
                    ini.Sections["Updater"].Keys["Updatetype"].Value = value.ToString();
            }
        }

        public int WaitTime
        {
            get
            {
                string sVal = ini.Sections["Updater"].Keys["WaitTime"].Value;
                int iVal = string.IsNullOrEmpty(sVal) ? 10 : int.Parse(sVal);
                return (iVal >= 5 && iVal <= 360) ? iVal : 10;
            }
            set
            {
                if (value >= 5 && value <= 360)
                    ini.Sections["Updater"].Keys["WaitTime"].Value = value.ToString();
            }
        }

        public int MaxUserWaitTime
        {
            get
            {
                string sVal = ini.Sections["Updater"].Keys["MaxUserWaitTime"].Value;
                int iVal = string.IsNullOrEmpty(sVal) ? 60 : int.Parse(sVal);
                return (iVal >= 5 && iVal <= 360) ? iVal : 60;
            }
            set
            {
                if (value >= 5 && value <= 360)
                    ini.Sections["Updater"].Keys["MaxUserWaitTime"].Value = value.ToString();
            }
        }

        public string StartParameters
        {
            get { return ini.Sections["Updater"].Keys["StartParameters"].Value; }
            set { ini.Sections["Updater"].Keys["StartParameters"].Value = value; }
        }

        public string SteamCmd
        {
            get { return ini.Sections["Paths"].Keys["SteamCmd"].Value; }
            set { ini.Sections["Paths"].Keys["SteamCmd"].Value = value; }
        }

        public string ArkFolder
        {
            get { return ini.Sections["Paths"].Keys["ArkFolder"].Value; }
            set { ini.Sections["Paths"].Keys["ArkFolder"].Value = value; }
        }

        public string UpdateFoundMessage
        {
            get { return ini.Sections["Messages"].Keys["UpdateFound"].Value; }
            set { ini.Sections["Messages"].Keys["UpdateFound"].Value = value; }
        }

        public string TenMinutesMessage
        {
            get { return ini.Sections["Messages"].Keys["TenMinutes"].Value; }
            set { ini.Sections["Messages"].Keys["TenMinutes"].Value = value; }
        }

        public string OneMinuteMessage
        {
            get { return ini.Sections["Messages"].Keys["OneMinute"].Value; }
            set { ini.Sections["Messages"].Keys["OneMinute"].Value = value; }
        }

        public Settings()
        {
            ini = new IniFile();
            Load();
        }

        public void Load()
        {
            // Read Settings
            if (File.Exists(IniFile))
                ini.Load(IniFile);

            if (!ini.Sections.Contains("UpdateChecker"))
                ini.Sections.Add("UpdateChecker");

            IniSection section = ini.Sections["UpdateChecker"];

            if (!section.Keys.Contains("CheckInterval"))
                section.Keys.Add("CheckInterval", "10").TrailingComment.Text = "Interval in minutes to check for updates (Min. 5, Max. 60)";
            if (!section.Keys.Contains("ServerHost"))
                section.Keys.Add("ServerHost", "127.0.0.1").TrailingComment.Text = "Hostname or IP of the server";
            if (!section.Keys.Contains("QueryPort"))
                section.Keys.Add("QueryPort", "27015").TrailingComment.Text = "Query port of the server";
            if (!section.Keys.Contains("RconPort"))
                section.Keys.Add("RconPort", "27020").TrailingComment.Text = "Rcon port of the server";
            if (!section.Keys.Contains("RconPassword"))
                section.Keys.Add("RconPassword", "").TrailingComment.Text = "Rcon password of the server";
            if (!section.Keys.Contains("ArkAppId"))
                section.Keys.Add("ArkAppId", "376030").TrailingComment.Text = "AppId of the ARK server. No need to change this in most cases.";
            if (!section.Keys.Contains("InstalledRevision"))
                section.Keys.Add("InstalledRevision", "0").TrailingComment.Text = "Currently installed revision of ARK" + Environment.NewLine + "DO NOT CHANGE THIS! THIS IS NOT EQUIVALENT TO THE VERSION!";

            if (!ini.Sections.Contains("Updater"))
                ini.Sections.Add("Updater");

            section = ini.Sections["Updater"];
            section.LeadingComment.EmptyLinesBefore = 1;

            if (!section.Keys.Contains("UpdateType"))
                section.Keys.Add("UpdateType", "2").TrailingComment.Text = "Type to define when to update if an update occurs" + Environment.NewLine + "1 = Immediately (message to use, wait 1 min. update)" + Environment.NewLine + "2 = After a given time" + Environment.NewLine + "3 = When the last user left";
            if (!section.Keys.Contains("WaitTime"))
                section.Keys.Add("WaitTime", "10").TrailingComment.Text = "Only if UpdateType = 2" + Environment.NewLine + "Time to wait until the server updates in minutes (Min. 5, Max. 360)";
            if (!section.Keys.Contains("MaxUserWaitTime"))
                section.Keys.Add("MaxUserWaitTime", "10").TrailingComment.Text = "Only if UpdateType = 3" + Environment.NewLine + "Time to wait until the server updates if still not all users have left the server (Min. 5, Max. 360)";
            if (!section.Keys.Contains("StartParameters"))
                section.Keys.Add("StartParameters", "TheIsland?QueryPort=27015?Port=7778?listen").TrailingComment.Text = "Enter the parameters you use to start the ARK server here." + Environment.NewLine + "These will be used to start the server after an update.";

            if (!ini.Sections.Contains("Paths"))
                ini.Sections.Add("Paths");

            section = ini.Sections["Paths"];
            section.LeadingComment.EmptyLinesBefore = 1;

            if (!section.Keys.Contains("SteamCmd"))
                section.Keys.Add("SteamCmd", "C:\\Steam\\steamcmd.exe").TrailingComment.Text = "Path to your steamcmd.exe";
            if (!section.Keys.Contains("ArkFolder"))
                section.Keys.Add("ArkFolder", "C:\\ARK").TrailingComment.Text = "Path to your ARK Server root directory. (Containing ShooterGame and Engine)";

            if (!ini.Sections.Contains("Messages"))
                ini.Sections.Add("Messages");

            section = ini.Sections["Messages"];
            section.LeadingComment.EmptyLinesBefore = 1;

            if (!section.Keys.Contains("UpdateFound"))
                section.Keys.Add("UpdateFound", "Update found. Automatic update will start in %minutes% minutes!").TrailingComment.Text = "Message to be sent when an update is found.";
            if (!section.Keys.Contains("TenMinutes"))
                section.Keys.Add("TenMinutes", "Automatic update will start in %minutes% minutes!").TrailingComment.Text = "Message to be sent when the 10 minute mark is reached.";
            if (!section.Keys.Contains("OneMinute"))
                section.Keys.Add("OneMinute", "Automatic update will start any minute!").TrailingComment.Text = "Message to be sent when the 1 minute mark is reached.";

            Save();
        }

        public void Save()
        {
            try
            {
                ini.Save(IniFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Settings could not be saved.");
                Console.WriteLine(ex.ToString());
            }
        }
    }
}

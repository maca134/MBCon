using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace Proxy.BE
{
    public class Command
    {
        public enum CmdType
        {
            [Description("#init")]
            Init = 0,
            [Description("#shutdown")]
            Shutdown = 1,
            [Description("#reassign")]
            Reassign = 2,
            [Description("#restart")]
            Restart = 3,
            [Description("#lock")]
            Lock = 4,
            [Description("#unlock")]
            Unlock = 5,
            [Description("#mission ")]
            Mission = 6,
            [Description("missions")]
            Missions = 7,
            [Description("RConPassword ")]
            RConPassword = 8,
            [Description("MaxPing ")]
            MaxPing = 9,
            [Description("kick ")]
            Kick = 10,
            [Description("players")]
            Players = 11,
            [Description("Say ")]
            Say = 12,
            [Description("loadBans")]
            LoadBans = 13,
            [Description("loadScripts")]
            LoadScripts = 14,
            [Description("loadEvents")]
            loadEvents = 15,
            [Description("bans")]
            Bans = 16,
            [Description("ban ")]
            Ban = 17,
            [Description("addBan ")]
            AddBan = 18,
            [Description("removeBan ")]
            RemoveBan = 19,
            [Description("writeBans")]
            WriteBans = 20,
            [Description("admins")]
            admins = 21,
            [Description("NULL")]
            Null = 999,

        }
        public CmdType Type = CmdType.Null;
        public string Parameters = "";

        public Command()
        {

        }

        public Command(string command)
        {
            Parse(command);
        }

        private void Parse(string command)
        {
            var values = Enum.GetValues(typeof(CmdType)).Cast<CmdType>();
            var desc = "";
            var type = CmdType.Null;
            var ci = new CultureInfo("en-US");

            foreach (var t in values)
            {
                desc = Utils.GetEnumDescription(t, "");
                if (desc != "" && command.StartsWith(desc, true, ci))
                {
                    type = t;
                    break;
                }
            }
            if (type == CmdType.Null || desc == "")
            {
                throw new CommandException("Failed to parse BECommand");
            }

            var param = command.Remove(0, desc.Length);
            Type = type;
            Parameters = param;
        }
    }
}

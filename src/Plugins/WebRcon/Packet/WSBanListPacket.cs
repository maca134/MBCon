using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Proxy;

namespace WebRcon.Packet
{
    public class WSBanListPacket : WSPacket
    {
        public List<Dictionary<string, string>> bans = new List<Dictionary<string, string>>();
        private readonly Regex rx = new Regex(@"^(?<data>[^\s]+)\s+(?<time>[\-0-9]+)(?<reason>.*)$");

        public WSBanListPacket(string[] bansfile)
            : base(4)
        {
            var i = 0;
            foreach (var line in bansfile)
            {
                var ban = line.Trim();
                try
                {
                    var row = new Dictionary<string, string>();
                    if (rx.IsMatch(ban))
                    {
                        var match = rx.Match(ban);
                        row.Add("data", match.Groups["data"].Value);
                        row.Add("time", match.Groups["time"].Value);
                        try
                        {
                            row.Add("reason", match.Groups["reason"].Value);
                        }
                        catch
                        {
                            row.Add("reason", "");
                        }
                    }
                    else
                    {
                        throw new ArgumentException("Input string is not valid");
                    }

                    row.Add("id", i.ToString());
                    bans.Add(row);
                }
                catch (Exception ex)
                {
                    AppConsole.Log(String.Format("Error parsing line '{0}'. {1}", ban, ex.Message), ConsoleColor.Red);
                }

                i++;
            }
        }
    }
}

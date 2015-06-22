using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Proxy.BE;

namespace Proxy
{
    public class Player
    {
        private static readonly Regex _playerLine = new Regex(@"^([0-9]+)\s+([0-9\.]+):([0-9]+)\s+([0-9]+)\s+([a-f0-9]+)(\(OK\))?\s+(.*)$");
        public static List<Player> ParsePlayers(Message message)
        {
            var _list = new List<Player>();

            var lines = message.Content.Split(Environment.NewLine.ToCharArray());

            foreach (var line in lines)
            {
                if (line == "Players on server:")
                    continue;

                if (line == "[#] [IP Address]:[Port] [Ping] [GUID] [Name]")
                    continue;

                if (line == "--------------------------------------------------")
                    continue;

                if (line.Contains("players in total)"))
                    continue;

                Player player;
                try
                {
                    player = new Player(line);
                }
                catch (Exception)
                {
                    continue;
                }

                _list.Add(player);
            }
            return _list;
        }

        private Player(string line)
        {
            if (!_playerLine.IsMatch(line))
            {
                throw new ArgumentException("Input string is not valid");
            }
            var match = _playerLine.Match(line);
            try
            {
                _playerId = Convert.ToInt16(match.Groups[1].ToString());
                _ip = match.Groups[2].ToString();
                _ping = Convert.ToInt16(match.Groups[4].ToString());
                _guid = match.Groups[5].ToString();
                _name = match.Groups[7].ToString();
            }
            catch (Exception ex)
            {
                throw new ArgumentException(String.Format("Input string is not valid {0}", ex.Message));
            }
        }

        private readonly int _playerId;
        public int PlayerId
        {
            get
            {
                return _playerId;
            }
        }

        private readonly string _ip;
        public string Ip
        {
            get
            {
                return _ip;
            }
        }

        private readonly int _ping;
        public int Ping
        {
            get
            {
                return _ping;
            }
        }

        private readonly string _guid;
        public string Guid
        {
            get
            {
                return _guid;
            }
        }

        private readonly string _name;
        public string Name
        {
            get
            {
                return _name;
            }
        }

    }
}

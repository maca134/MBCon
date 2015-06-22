using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Proxy.BE
{
    public class Client
    {
        private static readonly Regex _clientLine = new Regex(@"^([0-9]+)\s+([0-9\.]+):([0-9]+)$");
        
        public static List<Client> ParseClients(Message message)
        {
            var _list = new List<Client>();

            var lines = message.Content.Split(Environment.NewLine.ToCharArray());

            foreach (var line in lines)
            {
                Client client;
                try
                {
                    client = new Client(line);
                }
                catch
                {
                    continue;
                }

                _list.Add(client);
            }
            return _list;
        }

        public Client(string line)
        {
            if (!_clientLine.IsMatch(line))
            {
                throw new ArgumentException("Input string is not valid");
            }
            var match = _clientLine.Match(line);
            try
            {
                _clientId = Convert.ToInt16(match.Groups[1].ToString());
                _ip = match.Groups[2].ToString();
            }
            catch (Exception ex)
            {
                throw new ArgumentException(String.Format("Input string is not valid {0}", ex.Message));
            }
        }

        private readonly int _clientId;
        public int ClientId
        {
            get
            {
                return _clientId;
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
    }
}

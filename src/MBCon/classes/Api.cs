using System;
using System.Collections.Generic;
using System.Threading;
using System.Timers;
using BattleNET;
using Proxy;
using Proxy.BE;
using Proxy.DB;
using Timer = System.Timers.Timer;

namespace MBCon.classes
{
    public class Api : IApi, IDisposable
    {
        public event BeConnectedEventHandler OnBeConnectedEvent;
        public event BeDisconnectedEventHandler OnBeDisconnectedEvent;
        public event BeMessageReceivedEventHandler OnBeMessageReceivedEvent;
        public event PlayerListUpdatedEventHandler OnPlayerListUpdatedEvent;

        private List<Player> _players = new List<Player>();
        public List<Player> Players
        {
            get
            {
                return _players;
            }
        }

        private List<Client> _clients = new List<Client>();
        public List<Client> Clients
        {
            get
            {
                return _clients;
            }
        }

        private readonly DB _db;
        public IDB Db
        {
            get
            {
                return _db;
            }
        }

        private readonly ISettings _settings;
        public ISettings Settings
        {
            get
            {
                return _settings;
            }
        }

        private readonly BattlEyeClient _beclient;
        private readonly Dictionary<int, Message> _commandResponses = new Dictionary<int, Message>();
        private readonly Timer _getPlayersTimer = new Timer();

        public Api(BattlEyeClient client, ISettings settings)
        {
            _db = new DB();
            _beclient = client;
            _settings = settings;

            _beclient.BattlEyeConnected += BattlEyeConnected;
            _beclient.BattlEyeDisconnected += BattlEyeDisconnected;
            _beclient.BattlEyeMessageReceived += BattlEyeMessageReceived;

            _getPlayersTimer.Interval = 1000;
            _getPlayersTimer.Elapsed += getPlayersTimer_Elapsed;
            _getPlayersTimer.Enabled = true;
        }

        void getPlayersTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _getPlayersTimer.Interval = 10000;
            _getPlayersTimer.Enabled = false;
            GetPlayerList();
            _getPlayersTimer.Enabled = true;
        }

        private void GetPlayerList()
        {
            var response = SendCommandRes(new Command()
            {
                Type = Command.CmdType.Players
            });
            var newplayers = Player.ParsePlayers(response);
            if (newplayers.Count > 0 || _players.Count < 10)
            {
                _players = newplayers;
                if (OnPlayerListUpdatedEvent != null)
                {
                    OnPlayerListUpdatedEvent(_players);
                }
            }
            response = SendCommandRes(new Command()
            {
                Type = Command.CmdType.admins
            });
            _clients = Client.ParseClients(response);

            Console.Title = String.Format("MBCon - {0} Players Online | {1} BE Clients", _players.Count, _clients.Count);
        }

        void BattlEyeMessageReceived(BattlEyeMessageEventArgs args)
        {
            if (OnBeMessageReceivedEvent != null)
            {
                var message = new Message(args.Message, args.Id);
                if (args.Id != 256)
                {
                    _commandResponses[args.Id] = message;
                }
                else
                {
                    OnBeMessageReceivedEvent(message);
                }
            }
        }

        void BattlEyeDisconnected(BattlEyeDisconnectEventArgs args)
        {
            if (OnBeDisconnectedEvent != null)
            {
                OnBeDisconnectedEvent();
            }
        }

        void BattlEyeConnected(BattlEyeConnectEventArgs args)
        {
            if (OnBeConnectedEvent != null)
            {
                OnBeConnectedEvent();
            }
        }

        public int SendCommand(Command command)
        {
            return _beclient.SendCommand((BattlEyeCommand)command.Type, command.Parameters);
        }

        public Message SendCommandRes(Command command)
        {
            var id = SendCommand(command);
            Message response = null;
            while (response == null)
            {
                response = GetCommandResponse(id);
                Thread.Sleep(100);
            }
            return response;
        }

        public Message GetCommandResponse(int id)
        {
            if (!_commandResponses.ContainsKey(id))
            {
                return null;
            }
            return _commandResponses[id];
        }

        public void Dispose()
        {
            _getPlayersTimer.Dispose();
        }
    }
}

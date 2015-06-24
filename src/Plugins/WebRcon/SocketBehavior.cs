using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Proxy;
using Proxy.BE;
using WebRcon.Packet;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace WebRcon
{
    public class SocketBehavior : WebSocketBehavior
    {
        private bool _authed;
        private string _token = "";
        private readonly IApi _api;
        private readonly string _password;

        public SocketBehavior(IApi api, string password)
        {
            _api = api;
            _password = password;
        }

        protected override void OnOpen()
        {
            AppConsole.Log("New user connected to WebCon.");
            base.OnOpen();
        }

        protected override void OnClose(CloseEventArgs e)
        {
            AppConsole.Log(String.Format("User disconnected from WebCon. {0}", e.Reason));
            CloseConnection();
        }

        private void CloseConnection()
        {
            AppConsole.Log("Removing BE event.");
            _api.OnBeMessageReceivedEvent -= onBEMessageReceivedEvent;
            _api.OnPlayerListUpdatedEvent -= onPlayerListUpdatedEvent;
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            dynamic json = JsonConvert.DeserializeObject<dynamic>(e.Data);
            int type;
            try
            {
                type = Convert.ToInt16(json.type);
            }
            catch
            {
                return;
            }

            switch (type)
            {
                case 0:
                    ProcessLogin(JsonConvert.DeserializeObject<LoginRequest>(e.Data));
                    break;
                case 1:
                    ProcessCommand(JsonConvert.DeserializeObject<CommandRequest>(e.Data));
                    break;
                case 4:
                    GetBans(JsonConvert.DeserializeObject<BanListRequest>(e.Data));
                    break;
                case 5:
                    GetBEClients(JsonConvert.DeserializeObject<ClientListRequest>(e.Data));
                    break;
            }
        }

        private void ProcessLogin(LoginRequest req)
        {
            if (req.password == _password)
            {
                SendLoginResult(true);
                SendToken();
                _authed = true;
                _api.OnBeMessageReceivedEvent += onBEMessageReceivedEvent;
                SendPlayerList(_api.Players);
                _api.OnPlayerListUpdatedEvent += onPlayerListUpdatedEvent;
            }
            else
            {
                AppConsole.Log("User tried tried to login with the wrong password.");
                SendLoginResult(false);
            }
        }

        private void ProcessCommand(CommandRequest req)
        {
            if (req.token != _token)
            {
                _authed = false;
                return;
            }
            SendToken();

            foreach (var cmd in req.data)
            {
                RunCommand(cmd);
            }
        }

        private void RunCommand(string cmd)
        {
            AppConsole.Log(String.Format("New Command From WebRCON: {0}", cmd), ConsoleColor.Blue);
            var res = new WSBEMessage()
            {
                msgtype = "Log",
                received = DateTime.Now,
                message = "",
                type = 2
            };
            Command command;
            try
            {
                command = new Command(cmd);
            }
            catch (Exception ex)
            {
                AppConsole.Log(String.Format("Error parsing BECommand string: {0} - {1}", cmd, ex.Message), ConsoleColor.Red);
                res.message = "Error parsing command";
                Send(res.ToString());
                return;
            }
            _api.SendCommand(command);

            switch (command.Type)
            {
                case Command.CmdType.Lock:
                    res.message = "Server Locked";
                    break;
                case Command.CmdType.Unlock:
                    res.message = "Server Unlocked";
                    break;
                case Command.CmdType.MaxPing:
                    res.message = String.Format("Max Ping Set To {0}", command.Parameters);
                    break;
                case Command.CmdType.loadEvents:
                    res.message = "Reloading Events";
                    break;
                case Command.CmdType.LoadScripts:
                    res.message = "Reloading Scripts";
                    break;
                case Command.CmdType.LoadBans:
                    res.message = "Reloading Bans";
                    break;
                case Command.CmdType.RemoveBan:
                    res.message = "Removed ban";
                    break;
                case Command.CmdType.AddBan:
                    res.message = String.Format("Added Ban: {0}", command.Parameters);
                    break;
            }

            if (res.message != "")
            {
                Send(res.ToString());
            }
        }

        private void GetBans(BanListRequest req)
        {
            if (req.token != _token)
            {
                _authed = false;
                return;
            }
            SendToken();

            var banspath = Path.Combine(_api.Settings.BePath, "bans.txt");
            if (!File.Exists(banspath))
            {
                SendError("Could not load bans.txt");
                return;
            }

            var bans = File.ReadAllLines(banspath);
            var res = new WSBanListPacket(bans);
            Send(res.ToString());
        }

        private void GetBEClients(ClientListRequest req)
        {
            if (req.token != _token)
            {
                _authed = false;
                return;
            }
            SendToken();
            var res = new WSClientPacket()
            {
                clients = _api.Clients
            };
            Send(res.ToString());
        }

        void onPlayerListUpdatedEvent(List<Player> players)
        {
            SendPlayerList(players);
        }

        void onBEMessageReceivedEvent(Message message)
        {
            if (State == WebSocketState.Closed || State == WebSocketState.Closing)
            {
                CloseConnection();
                return;
            }

            var res = new WSBEMessage(message);
            Send(res.ToString());
        }

        private string GenerateToken()
        {
            _token = Guid.NewGuid().ToString();
            return _token;
        }

        private void SendLoginResult(bool success)
        {
            var res = new WSAuthPacket()
            {
                success = success
            };
            Send(res.ToString());
        }

        private void SendToken()
        {
            _token = Utils.GetRandomString();
            var res = new WSTokenPacket()
            {
                token = _token
            };
            Send(res.ToString());
        }

        private void SendPlayerList(List<Player> players)
        {
            var res = new WSPlayerPacket()
            {
                players = players
            };
            Send(res.ToString());
        }

        private void SendError(string error)
        {
            var res = new WSErrorPacket()
            {
                error = error
            };
            Send(res.ToString());
        }
    }
}

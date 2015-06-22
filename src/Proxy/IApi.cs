using System.Collections.Generic;
using Proxy.BE;
using Proxy.DB;

namespace Proxy
{

    public delegate void BeConnectedEventHandler();
    public delegate void BeDisconnectedEventHandler();
    public delegate void BeMessageReceivedEventHandler(Message message);
    public delegate void PlayerListUpdatedEventHandler(List<Player> players);

    public interface IApi
    {
        event BeConnectedEventHandler OnBeConnectedEvent;
        event BeDisconnectedEventHandler OnBeDisconnectedEvent;
        event BeMessageReceivedEventHandler OnBeMessageReceivedEvent;
        event PlayerListUpdatedEventHandler OnPlayerListUpdatedEvent;

        List<Player> Players { get; }
        List<Client> Clients { get; }

        IDB Db { get; }
        ISettings Settings { get; }

        int SendCommand(Command command);
        Message GetCommandResponse(int id);
    }
}

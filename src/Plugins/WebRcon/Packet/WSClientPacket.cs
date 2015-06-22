using System.Collections.Generic;
using Proxy.BE;

namespace WebRcon.Packet
{
    public class WSClientPacket : WSPacket
    {
        public List<Client> clients;
        public WSClientPacket() : base(5) { }
    }
}

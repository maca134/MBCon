using System.Collections.Generic;
using Proxy;

namespace WebRcon.Packet
{
    public class WSPlayerPacket : WSPacket
    {
        public List<Player> players;
        public WSPlayerPacket() : base(3) { }
    }
}

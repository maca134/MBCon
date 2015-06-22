using System.Collections.Generic;

namespace WebRcon.Packet
{
    public class CommandRequest : AuthToken
    {
        public List<string> data { get; set; }
    }
}

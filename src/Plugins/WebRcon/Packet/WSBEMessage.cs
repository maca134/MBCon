using System;
using Proxy.BE;

namespace WebRcon.Packet
{
    public class WSBEMessage : WSPacket
    {
        public string message;
        public string msgtype;
        public DateTime received;
        public WSBEMessage(Message message)
            : this()
        {
            this.message = message.Content;
            msgtype = message.Type.ToString();
            received = message.Received;
        }

        public WSBEMessage() : base(2) { }
    }
}

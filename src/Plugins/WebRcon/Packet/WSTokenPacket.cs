namespace WebRcon.Packet
{
    public class WSTokenPacket : WSPacket
    {
        public string token;
        public WSTokenPacket() : base(1) { }
    }
}

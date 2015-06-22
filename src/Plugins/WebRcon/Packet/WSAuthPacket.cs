namespace WebRcon.Packet
{
    class WSAuthPacket : WSPacket
    {
        public bool success;
        public WSAuthPacket() : base(0) { }
    }
}

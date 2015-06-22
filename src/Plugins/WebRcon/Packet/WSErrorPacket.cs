namespace WebRcon.Packet
{
    class WSErrorPacket : WSPacket
    {
        public string error;
        public WSErrorPacket() : base(-1) { }
    }
}

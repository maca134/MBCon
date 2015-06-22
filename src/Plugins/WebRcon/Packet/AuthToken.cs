namespace WebRcon.Packet
{
    abstract public class AuthToken
    {
        public int type { get; set; }
        public string token { get; set; }
    }
}

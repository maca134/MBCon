using Newtonsoft.Json;

namespace WebRcon.Packet
{
    abstract public class WSPacket
    {
        protected WSPacket(int type)
        {
            _type = type;
        }

        private int _type;
        public int type {
            get {
                return _type;
            }
            set {
                if (_type == -1)
                    _type = value;
            }
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}

using System;
using Proxy.BE;

namespace PlayerCheck
{
    public class Player
    {
        private readonly int _id;
        public int Id
        {
            get
            {
                return _id;
            }
        }

        private readonly string _name;
        public string Name
        {
            get
            {
                return _name;
            }
        }

        private readonly string _data;
        public string Data
        {
            get
            {
                return _data;
            }
        }

        public Player(Message message)
        {
            try
            {
                var groups = message.Match.Groups;
                _name = groups["Name"].Value;
                _id = Convert.ToInt16(groups["Id"].Value);
                _data = message.Type == Message.MessageType.ConnectIP ? groups["Ip"].Value : groups["Guid"].Value;
            }
            catch (Exception ex)
            {
                throw new PlayerCheckException(ex.Message);
            }
        }

    }
}

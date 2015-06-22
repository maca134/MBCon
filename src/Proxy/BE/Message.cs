using System;
using System.Text.RegularExpressions;

namespace Proxy.BE
{
    public class Message
    {
        public enum MessageType
        {
            Log,
            Chat,
            ConnectIP,
            ConnectGUID,
            ConnectLegacyGUID,
            Disconnect,
            Kick
        }

        private readonly MessageType _type = MessageType.Log;
        public MessageType Type
        {
            get
            {
                return _type;
            }
        }

        private readonly string _content;
        public string Content
        {
            get
            {
                return _content;
            }
        }

        private readonly int _id;
        public int Id
        {
            get
            {
                return _id;
            }
        }

        private readonly DateTime _received;
        public DateTime Received
        {
            get
            {
                return _received;
            }
        }

        private readonly Match _match;
        public Match Match
        {
            get
            {
                return _match;
            }
        }

        private readonly Regex _chatRegex = new Regex(@"^\((?<Channel>[^\)]+)\) (?<Name>[^:]+): (?<Message>.*)$");
        private readonly Regex _connectedIpRegex = new Regex(@"^Player #(?<Id>[0-9]+) (?<Name>.*) \((?<Ip>[0-9\.]+):[0-9]+\) connected$");
        private readonly Regex _connectedGuidRegex = new Regex(@"^Verified GUID \((?<Guid>[a-f0-9]{32})\) of player #(?<Id>[0-9]+) (?<Name>.*)$");
        private readonly Regex _connectedLegacyGuidRegex = new Regex(@"^Player #(?<Id>[0-9]+) (?<Name>[^:]+) \- Legacy GUID: (?<Guid>[a-f0-9]{32})$");
        private readonly Regex _disconnectedRegex = new Regex(@"^Player #(?<Id>[0-9]+) (?<Name>[^:]+) disconnected$");
        private readonly Regex _kickRegex = new Regex(@"^Player #(?<Id>[0-9\-]+) (?<Name>.*) \((?<Guid>[a-f0-9]{32})\) has been kicked by BattlEye: (?<Reason>.*)$");

        public Message(string message, int id)
        {
            _content = message;
            _id = id;
            _received = DateTime.Now;

            if (_chatRegex.IsMatch(_content))
            {
                _type = MessageType.Chat;
                _match = _chatRegex.Match(_content);
            }

            if (_connectedIpRegex.IsMatch(message))
            {
                _type = MessageType.ConnectIP;
                _match = _connectedIpRegex.Match(_content);
            }

            if (_connectedGuidRegex.IsMatch(message))
            {
                _type = MessageType.ConnectGUID;
                _match = _connectedGuidRegex.Match(_content);
            }

            if (_connectedLegacyGuidRegex.IsMatch(message))
            {
                _type = MessageType.ConnectLegacyGUID;
                _match = _connectedLegacyGuidRegex.Match(_content);
            }

            if (_kickRegex.IsMatch(message))
            {
                _type = MessageType.Kick;
                _match = _kickRegex.Match(_content);
            }

            if (_disconnectedRegex.IsMatch(message))
            {
                _type = MessageType.Disconnect;
                _match = _disconnectedRegex.Match(_content);
            }
        }

        public byte[] GetMessageBytes()
        {
            var bytes = new byte[_content.Length * sizeof(char)];
            Buffer.BlockCopy(_content.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }
    }
}

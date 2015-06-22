using System;
using Proxy;
using Proxy.BE;
using Proxy.Plugin;

namespace Console
{
    public class Console : PluginBase, IPlugin
    {
        public string Name
        {
            get
            {
                return "Console";
            }
        }

        public string Author
        {
            get
            {
                return "maca134";
            }
        }

        private IApi _api;

        public void Init(IApi api, string path)
        {
            _api = api;
            _api.OnBeMessageReceivedEvent += _be_MessageEventHandler;
        }

        void _be_MessageEventHandler(Message message)
        {
            var color = ConsoleColor.DarkGray;
            switch (message.Type)
            {
                case Message.MessageType.ConnectGUID:
                case Message.MessageType.ConnectIP:
                case Message.MessageType.ConnectLegacyGUID:
                    color = ConsoleColor.Blue;
                    break;
                case Message.MessageType.Disconnect:
                    color = ConsoleColor.DarkBlue;
                    break;
                case Message.MessageType.Kick:
                    color = ConsoleColor.Red;
                    break;
                case Message.MessageType.Chat:
                    color = ConsoleColor.White;
                    if (message.Content.StartsWith("(Side)"))
                        color = ConsoleColor.Cyan;
                    if (message.Content.StartsWith("(Vehicle)"))
                        color = ConsoleColor.Yellow;
                    if (message.Content.StartsWith("(Direct)"))
                        color = ConsoleColor.White;
                    if (message.Content.StartsWith("(Group)"))
                        color = ConsoleColor.Green;
                    if (message.Content.StartsWith("(Global)"))
                        color = ConsoleColor.Gray;
                    
                    break;
            }

            AppConsole.Log(message.Content, color);
        }

        public void Kill()
        {

        }
    }
}

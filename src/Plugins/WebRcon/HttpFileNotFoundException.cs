using System;

namespace WebRcon
{
    public class HttpFileNotFoundException : Exception
    {
        public HttpFileNotFoundException()
        {
        }

        public HttpFileNotFoundException(string message)
            : base(message)
        {
        }

        public HttpFileNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}

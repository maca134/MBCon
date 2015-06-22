using System;
using System.Net;
using System.Text;
using System.Threading;

namespace WebRcon
{
    public struct HttpResponse
    {
        public static HttpResponse Blank
        {
            get
            {
                return new HttpResponse() { Content = "", Mime = "text/plain" };
            }
        }
        public string Content;
        public string Mime;
    }

    public class WebServer : IDisposable
    {
        private readonly HttpListener _listener = new HttpListener();
        private readonly Func<HttpListenerRequest, HttpResponse> _responderMethod;

        public WebServer(string[] prefixes, Func<HttpListenerRequest, HttpResponse> method)
        {
            if (!HttpListener.IsSupported)
                throw new NotSupportedException(
                    "Needs Windows XP SP2, Server 2003 or later.");

            // URI prefixes are required, for example 
            // "http://localhost:8080/index/".
            if (prefixes == null || prefixes.Length == 0)
                throw new ArgumentException("prefixes");

            // A responder method is required
            if (method == null)
                throw new ArgumentException("method");

            foreach (var s in prefixes)
                _listener.Prefixes.Add(s);

            _responderMethod = method;
            try
            {
                _listener.Start();
            }
            catch (HttpListenerException ex)
            {
                throw new WebRconException(String.Format("Could not open HTTP port. (try running as admin). {0}", ex.Message));
            }
        }

        public WebServer(Func<HttpListenerRequest, HttpResponse> method, params string[] prefixes)
            : this(prefixes, method) { }

        public void Run()
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                //Console.WriteLine("Webserver running...");
                try
                {
                    while (_listener.IsListening)
                    {
                        ThreadPool.QueueUserWorkItem(c =>
                        {
                            var ctx = c as HttpListenerContext;
                            var response = HttpResponse.Blank;
                            try
                            {
                                response = _responderMethod(ctx.Request);
                            }
                            catch (HttpFileNotFoundException)
                            {
                                ctx.Response.StatusCode = 404;
                                response = new HttpResponse()
                                {
                                    Content = "File Not Found",
                                    Mime = "text/html"
                                };
                            }
                            catch (Exception)
                            {
                                ctx.Response.StatusCode = 500;
                                response = new HttpResponse()
                                {
                                    Content = "Server Error",
                                    Mime = "text/html"
                                };
                            }
                            finally
                            {
                                var buf = Encoding.UTF8.GetBytes(response.Content);
                                ctx.Response.ContentLength64 = buf.Length;
                                ctx.Response.ContentType = response.Mime;
                                ctx.Response.OutputStream.Write(buf, 0, buf.Length);
                                ctx.Response.OutputStream.Flush();
                                ctx.Response.OutputStream.Close();
                            }
                        }, _listener.GetContext());
                    }
                }
                catch
                {
                    // ignored
                } // suppress any exceptions
            });
        }

        public void Dispose() {
            Stop();
        }
        
        public void Stop()
        {
            _listener.Stop();
            _listener.Close();
        }
    }
}

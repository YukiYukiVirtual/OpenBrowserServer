using System;
using System.Net;
using OpenBrowserServer.Component;
using OpenBrowserServer.Logger;

namespace OpenBrowserServer.WebServer
{
    public partial class HttpServer
    {
        readonly HttpListener listener;
        readonly Settings settings;
        readonly History history;
        DateTime lastRequestTime;
        public HttpServer(Settings settings, History history)
        {
            this.settings = settings;
            this.history = history;

            lastRequestTime = DateTime.MinValue;

            listener = new HttpListener();

            listener.Prefixes.Add("http://localhost:21983/");

            listener.Start();
            listener.BeginGetContext(OnRequested, null);
        }
        ~HttpServer()
        {
            Console.WriteLine("HttpServer destructor");
            listener.Close();
        }

        private void OnRequested(IAsyncResult ar)
        {
            if (!listener.IsListening)
            {
                return;
            }
            listener.BeginGetContext(OnRequested, null);
            HttpListenerContext context = listener.EndGetContext(ar);
            WebRequest webRequestResult = RequestHandler(context);
            history.WriteLine($"Web Requested: '{context.Request.Url}' {webRequestResult}");
        }
        private string GetApiPath(string rawUrl)
        {
            string[] str = rawUrl.Split('/');
            //Console.WriteLine(str);
            foreach (string s in str) Console.WriteLine(s);
            //Console.WriteLine(str.Length - 1);
            if (rawUrl.IndexOf("Temporary_Listen_Addresses") != -1)
            {
                return str[2]; // /Temporary_Listen_Addresses/API (非推奨)
            }
            else
            {
                return str[1]; // /API/xxx.xyz
            }
        }

    }
}

using System;
using System.Net;
using OpenBrowserServer.Component;
using OpenBrowserServer.Logger;

namespace OpenBrowserServer.WebServer
{
    public partial class HttpServer
    {
        HttpListener listener;
        Settings settings;
        DateTime lastRequestTime;
        History history;
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
            try
            {
                listener.BeginGetContext(OnRequested, null);
                HttpListenerContext context = listener.EndGetContext(ar);
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;

                //Console.WriteLine("OnRequested");

                DateTime now = DateTime.Now;
                TimeSpan timeSpan = now - lastRequestTime;
                if (timeSpan.TotalMilliseconds >= settings.HttpRequestPeriod)
                {
                    //Console.WriteLine($"RawUrl:{request.RawUrl}");
                    switch (request.RawUrl)
                    {
                        case "/":
                            //Console.WriteLine("Root");
                            ProcessRoot(context);
                            break;
                        case "/favicon.ico":
                            //Console.WriteLine("favicon");
                            response.KeepAlive = false;
                            response.StatusCode = 404;
                            response.ContentLength64 = 0;
                            break;
                        default:
                            string apiPath = GetApiPath(request.RawUrl);
                            //Console.WriteLine($"API:{apiPath}.");
                            switch (apiPath)
                            {
                                case "keys":
                                    //Console.WriteLine($"/keysは廃止の可能性があります");
                                    ProcessKeys(context);
                                    break;
                                default:
                                    Console.WriteLine($"{apiPath}は未実装のAPIまたは不正なリクエスト");
                                    break;
                            }
                            break;
                    }
                    lastRequestTime = now;
                    history.WriteLine($"Http Requested: URL: '{request.RawUrl}' OK"); // TODO 関数化して、OpenURLと同じようにenumを返す
                }
                else
                {
                    history.WriteLine($"Http Requested: URL: '{request.RawUrl}' TimeSpanError");
                }
                response.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
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

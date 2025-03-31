using System.IO;
using System;
using System.Net;
using System.Text;

namespace OpenBrowserServer.WebServer
{
    public enum WebRequest
    {
        OK,
        TimeSpanError,
        Exception,
        NotImplemented,
    }
    public partial class HttpServer
    {
        private WebRequest RequestHandler(HttpListenerContext context)
        {
            WebRequest webRequestResult;
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            if (config.PauseSystem)
            {
                response.KeepAlive = false;
                response.StatusCode = 425;
                response.ContentLength64 = 0;
                return WebRequest.OK;
            }
            //Console.WriteLine("OnRequested");
            try
            {
                DateTime now = DateTime.Now;
                TimeSpan timeSpan = now - lastRequestTime;
                if (timeSpan.TotalMilliseconds >= config.Setting.HttpRequestPeriod)
                {
                    //Console.WriteLine($"RawUrl:{request.RawUrl}");
                    switch (request.RawUrl)
                    {
                        case "/":
                            //Console.WriteLine("Root");
                            webRequestResult = ProcessRoot(context);
                            lastRequestTime = now;
                            break;
                        case "/favicon.ico":
                            //Console.WriteLine("favicon");
                            response.KeepAlive = false;
                            response.StatusCode = 404;
                            response.ContentLength64 = 0;
                            webRequestResult = WebRequest.OK;
                            break;
                        default:
                            string apiPath = GetApiPath(request.RawUrl);
                            //Console.WriteLine($"API:{apiPath}.");
                            switch (apiPath)
                            {
                                case "keys":
                                    //Console.WriteLine($"/keysは廃止の可能性があります");
                                    webRequestResult = ProcessKeys(context);
                                    break;
                                default:
                                    history.WriteLine($"▲未実装のAPIまたは不正なリクエスト {apiPath}");
                                    webRequestResult = WebRequest.NotImplemented;
                                    break;
                            }
                            lastRequestTime = now;
                            break;
                    }
                }
                else
                {
                    webRequestResult = WebRequest.TimeSpanError;
                }
            }
            catch (Exception e)
            {
                history.WriteLine($"▲Httpリクエストで予期せぬ例外が発生 {request.RawUrl}");
                history.WriteLine(e.ToString());
                webRequestResult = WebRequest.Exception;
            }
            response.Close();
            return webRequestResult;
        }
        /// <summary>
        /// '/'にリクエストが来たときに応答する
        /// </summary>
        /// <param name="context"></param>
        private WebRequest ProcessRoot(HttpListenerContext context)
        {
            HttpListenerResponse response = context.Response;

            // レスポンスデータを匿名型で作成
            string[] versionParts = config.FileVersion.Replace("v","").Split('.');
            if(versionParts.Length != 3)
            {
                versionParts = new string[] { "0", "0", "0" };
            }
            var obj = new
            {
                version = new
                {
                    major = versionParts[0],
                    minor = versionParts[1],
                    build = versionParts[2],
                    full  = config.FileVersion,
                },
                edition = config.Edition,
            };
            // データをJSONにシリアライズ
            string content = System.Text.Json.JsonSerializer.Serialize(obj);
            // byteバッファに詰め替え
            byte[] buffer = Encoding.GetEncoding("UTF-8").GetBytes(content);

            // レスポンスヘッダ設定
            response.KeepAlive = false;
            response.StatusCode = 200;
            response.ContentType = "application/json";
            response.ContentLength64 = buffer.Length;

            // レスポンスにデータ書き込み
            response.OutputStream.Write(buffer, 0, buffer.Length);

            return WebRequest.OK;
        }
        /// <summary>
        /// '/keys/xxx'にリクエストが来たときに応答する
        /// </summary>
        /// <param name="context"></param>
        private WebRequest ProcessKeys(HttpListenerContext context)
        {
            WebRequest webRequestResult;
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;
            string filename = Path.GetFileName(request.RawUrl);
            string filepath = Path.Combine(config.WorkingPath, "keys", filename);
            string mimetype = System.Web.MimeMapping.GetMimeMapping(filename);

            try
            {
                using (FileStream filestream = new FileStream(filepath, FileMode.Open, FileAccess.Read))
                {
                    int len = (int)filestream.Length;
                    byte[] buf = new Byte[len];

                    filestream.Read(buf, 0, len);

                    response.KeepAlive = false;
                    response.StatusCode = 200;
                    response.ContentType = mimetype;
                    response.ContentLength64 = len;

                    if (request.HttpMethod.Equals("GET"))
                    {
                        response.OutputStream.Write(buf, 0, len);
                    }
                    else if (request.HttpMethod.Equals("HEAD"))
                    {
                        // NOP
                    }
                    webRequestResult = WebRequest.OK;
                }
            }
            catch (FileNotFoundException e)
            {
                response.KeepAlive = false;
                response.StatusCode = 404;
                response.ContentLength64 = 0;

                history.WriteLine($"▲ProcessKeys 要求されたファイルがありません {filepath}");
                history.WriteLine(e.ToString());
                webRequestResult = WebRequest.Exception;
            }
            catch (Exception e)
            {
                response.KeepAlive = false;
                response.StatusCode = 500;
                response.ContentLength64 = 0;

                history.WriteLine($"▲ProcessKeys 何らかの例外発生 {filepath}");
                Console.WriteLine(e.ToString());
                webRequestResult = WebRequest.Exception;
            }
            return webRequestResult;
        }
    }
}

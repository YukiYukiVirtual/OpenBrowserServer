using System;
using System.IO;
using System.Net;
using System.Text;

namespace OpenBrowserServer.WebServer
{
    public enum StatusCode
    {
        OK = 200, // コンテンツ設定完了
        Forbidden = 403,
        NotFound = 404,
        TooEarly = 425,
        InternalServerError = 500,
        ServiceUnavailable = 503,
    }
    public partial class HttpServer
    {
        private StatusCode RequestHandler(HttpListenerContext context)
        {
            StatusCode statusCode;
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            // faviconだけ例外的に処理する
            if (request.RawUrl == "/favicon.ico")
            {
                statusCode = StatusCode.NotFound;
            }
            else if (config.PauseSystem)
            {
                statusCode = StatusCode.ServiceUnavailable;
            }
            else
            {
                try
                {
                    DateTime now = DateTime.Now;
                    TimeSpan timeSpan = now - lastRequestTime;
                    if (timeSpan.TotalMilliseconds >= config.Setting.HttpRequestPeriod)
                    {
                        // アプリ情報表示
                        if (request.RawUrl == "/")
                        {
                            statusCode = ProcessRoot(context);
                        }
                        else
                        {
                            string apiPath = GetApiPath(request.RawUrl);
                            switch (apiPath)
                            {
                                case "keys":
                                    statusCode = ProcessKeys(context);
                                    break;
                                default:
                                    history.WriteLine($"▲未実装のAPIまたは不正なリクエスト {apiPath}");
                                    statusCode = StatusCode.NotFound;
                                    break;
                            }
                        }
                    }
                    else
                    {
                        statusCode = StatusCode.TooEarly;
                    }
                    lastRequestTime = now;
                }
                catch (Exception e)
                {
                    history.WriteLine($"▲Httpリクエストで予期せぬ例外が発生 {request.RawUrl}");
                    history.WriteLine(e.ToString());
                    statusCode = StatusCode.InternalServerError;
                }
            }

            if(statusCode != StatusCode.OK)
            {
                string str = $"{(int)statusCode} {statusCode.ToString()}";
                byte[] buffer = Encoding.GetEncoding("UTF-8").GetBytes(str);

                response.KeepAlive = false;
                response.ContentType = "text/plain";
                response.ContentLength64 = buffer.Length;
                response.StatusCode = (int)statusCode;
                response.OutputStream.Write(buffer, 0, buffer.Length);
            }

            response.Close();
            return statusCode;
        }
        /// <summary>
        /// '/'にリクエストが来たときに応答する
        /// </summary>
        /// <param name="context"></param>
        private StatusCode ProcessRoot(HttpListenerContext context)
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

            return StatusCode.OK;
        }
        /// <summary>
        /// '/keys/xxx'にリクエストが来たときに応答する
        /// </summary>
        /// <param name="context"></param>
        private StatusCode ProcessKeys(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;
            string filename = Path.GetFileName(request.RawUrl);
            string filepath = Path.Combine(config.WorkingPath, "keys", filename);
            string mimetype = System.Web.MimeMapping.GetMimeMapping(filename);

            if (!File.Exists(filepath))
            {
                history.WriteLine($"▲ProcessKeys 要求されたファイルがありません {filepath}");
                return StatusCode.NotFound;
            }
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
                }
                return StatusCode.OK;
            }
            catch (Exception e)
            {
                history.WriteLine($"▲ProcessKeys 何らかの例外発生 {filepath}");
                history.WriteLine(e.ToString());
                return StatusCode.InternalServerError;
            }
        }
    }
}

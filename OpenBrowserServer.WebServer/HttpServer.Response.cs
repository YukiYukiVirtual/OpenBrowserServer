using System.IO;
using System;
using System.Net;
using System.Text;

namespace OpenBrowserServer.WebServer
{
    public partial class HttpServer
    {
        private void ProcessRoot(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;
            string content = $"{{\"version\": {{\"major\": {settings.fileVersionInfo.FileMajorPart}, \"minor\": {settings.fileVersionInfo.FileMinorPart}, \"private\": {settings.fileVersionInfo.FilePrivatePart}, \"full\": \"{settings.FileVersion}\" }}, \"build\": \"{settings.Build}\"}}";
            byte[] buffer = Encoding.GetEncoding("UTF-8").GetBytes(content);
            response.KeepAlive = false;
            response.StatusCode = 200;
            response.ContentType = "application/json";
            response.ContentLength64 = buffer.Length;

            response.OutputStream.Write(buffer, 0, buffer.Length);
        }
        private void ProcessKeys(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;
            string filename = Path.GetFileName(request.RawUrl);
            string filepath = "keys/" + filename;
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
                }
            }
            catch (FileNotFoundException e)
            {
                response.KeepAlive = false;
                response.StatusCode = 404;
                response.ContentLength64 = 0;

                Console.WriteLine(e.ToString());
            }
            catch (Exception e)
            {
                response.KeepAlive = false;
                response.StatusCode = 500;
                response.ContentLength64 = 0;

                Console.WriteLine(e.ToString());
            }
        }
    }
}

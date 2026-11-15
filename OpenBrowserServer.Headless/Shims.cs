// Ubuntu(ヘッドレス)ビルド用の最小シム。
// .NET Framework にしか無い System.Web.MimeMapping を、HttpServer.Response.cs が
// そのままコンパイルできるよう同名・同シグネチャで置き換える。
namespace System.Web
{
    public static class MimeMapping
    {
        public static string GetMimeMapping(string fileName)
        {
            string ext = System.IO.Path.GetExtension(fileName);
            if (ext == null)
            {
                return "application/octet-stream";
            }
            switch (ext.ToLowerInvariant())
            {
                case ".txt": return "text/plain";
                case ".json": return "application/json";
                case ".png": return "image/png";
                case ".jpg":
                case ".jpeg": return "image/jpeg";
                case ".gif": return "image/gif";
                case ".bmp": return "image/bmp";
                case ".mp4": return "video/mp4";
                case ".webm": return "video/webm";
                case ".mp3": return "audio/mpeg";
                case ".wav": return "audio/wav";
                case ".ogg": return "audio/ogg";
                case ".html": return "text/html";
                default: return "application/octet-stream";
            }
        }
    }
}

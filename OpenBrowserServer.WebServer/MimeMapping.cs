using System;
using System.Collections.Generic;

namespace System.Web
{
    // .NET Framework の System.Web.MimeMapping の最小シム。
    // モダン.NET には同等APIが無いため、HttpServer.Response が使う拡張子分だけを実装する。
    public static class MimeMapping
    {
        private static readonly Dictionary<string, string> Map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [".html"] = "text/html",
            [".htm"] = "text/html",
            [".txt"] = "text/plain",
            [".json"] = "application/json",
            [".png"] = "image/png",
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".gif"] = "image/gif",
            [".ico"] = "image/x-icon",
            [".svg"] = "image/svg+xml",
            [".css"] = "text/css",
            [".js"] = "application/javascript",
            [".xml"] = "text/xml",
            [".pdf"] = "application/pdf",
            [".zip"] = "application/zip",
        };

        public static string GetMimeMapping(string fileName)
        {
            string ext = System.IO.Path.GetExtension(fileName);
            if (ext != null && Map.TryGetValue(ext, out string mime))
            {
                return mime;
            }
            return "application/octet-stream";
        }
    }
}

using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace OpenBrowserServer.Component
{
    public class JsonDownloader
    {
        // キャッシュ
        static string CachedUrl = "";
        static string CachedWorldName = "";
        static Image CachedThumbnailImage = null;

        public static bool GetWorldInformation(
            string url,
            out string worldName,
            out Image thumbnailImage)
        {
            if (url == CachedUrl)
            {
                Console.WriteLine("Cached {CachedUrl}");
            }
            else
            {
                using (HttpClient client = new HttpClient())
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                    try
                    {

                        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.3; Trident/7.0; rv:11.0) like Gecko");

                        // JSONダウンロード
                        string responseBody = client.GetStringAsync(url).Result;
                        JsonElement jsonDocument = JsonDocument.Parse(responseBody).RootElement;


                        // サムネイル
                        string imageUrl = jsonDocument.GetProperty("thumbnailImageUrl").GetString();
                        Stream stream = client.GetStreamAsync(imageUrl).Result;
                        CachedThumbnailImage = Image.FromStream(stream);

                        // ワールド名
                        CachedWorldName = jsonDocument.GetProperty("name").GetString();

                        CachedUrl = url;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                        worldName = "";
                        thumbnailImage = null;
                        return false;
                    }

                }
            }
            worldName = CachedWorldName;
            thumbnailImage = CachedThumbnailImage;
            return true;
        }
    }
}

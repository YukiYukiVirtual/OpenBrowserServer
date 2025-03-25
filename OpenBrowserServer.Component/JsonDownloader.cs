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
        public static string CachedWorldId { get; private set; }
        public static string CachedWorldName { get; private set; }
        public static string CachedAuthorId { get; private set; }
        public static string CachedAuthorName { get; private set; }
        public static string CachedDescription { get; private set; }
        public static Image  CachedThumbnailImage { get; private set; }

        public static bool CacheWorldInformation(
            string worldId)
        {
            if (worldId == CachedWorldId)
            {
                Console.WriteLine($"Cached {CachedWorldId}");
            }
            else
            {
                string url = $"https://api.vrchat.cloud/api/1/worlds/{worldId}";
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

                        CachedWorldName = jsonDocument.GetProperty("name").GetString();
                        CachedDescription = jsonDocument.GetProperty("description").GetString();
                        CachedAuthorId = jsonDocument.GetProperty("authorId").GetString();
                        CachedAuthorName = jsonDocument.GetProperty("authorName").GetString();

                        // 最後まで設定が完了してからキャッシュする
                        CachedWorldId = worldId;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                        return false;
                    }

                }
            }
            return true;
        }
    }
}

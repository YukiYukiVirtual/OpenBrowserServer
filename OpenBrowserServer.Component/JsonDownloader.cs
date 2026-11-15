using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;

namespace OpenBrowserServer.Component
{
    public class JsonDownloader
    {
        // キャッシュ
        public static string UserAgent { get; set; }
        public static string CachedWorldId { get; private set; }
        public static string CachedWorldName { get; private set; }
        public static string CachedAuthorId { get; private set; }
        public static string CachedAuthorName { get; private set; }
        public static string CachedDescription { get; private set; }
        public static Image  CachedThumbnailImage { get; private set; }

        public static bool CacheWorldInformation(
            string worldId)
        {
#if UBUNTU
            // Ubuntu(ヘッドレス)ビルド用: VRChat API は叩かず、ダミーのワールド情報を返す。
            // BAN判定のテストができるよう、作者ID/名前は環境変数で差し替え可能。
            CachedWorldName = "Dummy World";
            CachedDescription = "Dummy world information for Ubuntu headless test.";
            CachedAuthorId = Environment.GetEnvironmentVariable("OPENBROWSER_DUMMY_AUTHOR_ID") ?? "usr_00000000-0000-0000-0000-000000000000";
            CachedAuthorName = Environment.GetEnvironmentVariable("OPENBROWSER_DUMMY_AUTHOR_NAME") ?? "DummyAuthor";
            CachedWorldId = worldId;
            return true;
#else
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

                        client.DefaultRequestHeaders.Add("User-Agent", UserAgent);

                        // JSONダウンロード
                        string responseBody = client.GetStringAsync(url).Result;
                        JsonElement jsonDocument = JsonDocument.Parse(responseBody).RootElement;

                        // サムネイル
                        string imageUrl = jsonDocument.GetProperty("thumbnailImageUrl").GetString();
#if !UBUNTU
                        Stream stream = client.GetStreamAsync(imageUrl).Result;
                        CachedThumbnailImage = Image.FromStream(stream);
#else
                        // Ubuntu(ヘッドレス)ビルド用: System.Drawing は使えないためサムネイル取得を省略する
                        _ = imageUrl;
#endif

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
#endif
        }
    }
}

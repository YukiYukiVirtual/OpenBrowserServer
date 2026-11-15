using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

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
            // ログ監視スレッドなど、同期的に呼びたい場所向けのラッパー
            return CacheWorldInformationAsync(worldId).GetAwaiter().GetResult();
        }
        public static async Task<bool> CacheWorldInformationAsync(
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
                    try
                    {

                        client.DefaultRequestHeaders.Add("User-Agent", UserAgent);

                        // JSONダウンロード
                        string responseBody = await client.GetStringAsync(url);
                        JsonElement jsonDocument = JsonDocument.Parse(responseBody).RootElement;

                        // 取得した値はまずローカル変数に受け、全部そろってからキャッシュへ反映する
                        // (途中で失敗した時に、キャッシュが中途半端な状態にならないようにするため。
                        //  失敗時は false を返し、呼び出し側が取り直せるようにする)
                        // サムネイル
                        string imageUrl = jsonDocument.GetProperty("thumbnailImageUrl").GetString();
#if !UBUNTU
                        Stream stream = await client.GetStreamAsync(imageUrl);
                        Image thumbnailImage = Image.FromStream(stream);
#else
                        // Ubuntu(ヘッドレス)ビルド用: System.Drawing は使えないためサムネイル取得を省略する
                        _ = imageUrl;
#endif

                        string worldName = jsonDocument.GetProperty("name").GetString();
                        string description = jsonDocument.GetProperty("description").GetString();
                        string authorId = jsonDocument.GetProperty("authorId").GetString();
                        string authorName = jsonDocument.GetProperty("authorName").GetString();

                        // 最後まで取得が完了してからキャッシュする
#if !UBUNTU
                        CachedThumbnailImage = thumbnailImage;
#endif
                        CachedWorldName = worldName;
                        CachedDescription = description;
                        CachedAuthorId = authorId;
                        CachedAuthorName = authorName;
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

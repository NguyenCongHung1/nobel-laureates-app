using System.Net;
using Microsoft.Extensions.Caching.Memory;

namespace NobelLaureatesApp.Helper
{
    public class PastebinHelper
    {
        private static readonly HttpClient httpClient = new HttpClient(new HttpClientHandler
        {
            UseProxy = true,
            Proxy = WebRequest.GetSystemWebProxy(),
        });
        private static readonly string apiPostURl = "https://pastebin.com/api/api_post.php";
        private static readonly string apiLoginUrl = "https://pastebin.com/api/api_login.php";
        private static readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
        private const string CACHE_KEY = "PastebinUserKey";


        public static async Task<string> CreatePastebinAsync(string content, string apiDevKey, string apiUserKey = "", string title = "Nobel Report", bool isPublic = true)
        {
            string pastePrivacy = isPublic ? "0" : "1";
            var values = new Dictionary<string, string>
            {
                { "api_dev_key", apiDevKey },
                { "api_option", "paste" },
                { "api_paste_code", content },
                { "api_paste_name", title },
                { "api_paste_private", pastePrivacy },
                { "api_paste_expire_date", "N" }
            };

            if (!string.IsNullOrEmpty(apiUserKey))
            {
                values.Add("api_user_key", apiUserKey);
            }

            var contentPost = new FormUrlEncodedContent(values);
            var response = await httpClient.PostAsync(apiPostURl, contentPost);;
            response.EnsureSuccessStatusCode();

            string result = await response.Content.ReadAsStringAsync();

            if (result.Contains("https://pastebin.com/")) return result;

            throw new Exception($"Error created Paste: {result}");
        }

        public static async Task<bool> DeletePastebinAsync(string pasteKey, string apiDevKey, string apiUserKey)
        {
            var values = new Dictionary<string, string>
            {
                { "api_dev_key", apiDevKey },
                { "api_user_key", apiUserKey },
                { "api_option", "delete" },
                { "api_paste_key", pasteKey }
            };

            var contentPost = new FormUrlEncodedContent(values);
            var response = await httpClient.PostAsync(apiPostURl, contentPost);

            string result = await response.Content.ReadAsStringAsync();

            if (result.ToLower().Contains("post deleted") || result.ToLower().Contains("paste removed"))
            {
                return true;
            }

            Console.WriteLine($"Error delete Paste: {result}");
            return false;
        }

        public static async Task<string> GetUserKeyAsync(string apiDevKey, string username, string password)
        {
            var values = new Dictionary<string, string>
            {
                { "api_dev_key", apiDevKey },
                { "api_user_name", username },
                { "api_user_password", password }
            };

            var contentPost = new FormUrlEncodedContent(values);
            var response = await httpClient.PostAsync(apiLoginUrl, contentPost);

            string result = await response.Content.ReadAsStringAsync();

            if (result.Contains("Bad API request")) throw new Exception("Login failed: " + result);
            return result;
        }

        public static async Task<string> GetUserKeyWithCacheAsync(string apiDevKey, string username, string password)
        {
            if (!_cache.TryGetValue(CACHE_KEY, out string userKey))
            {
                Console.WriteLine(">>> Cache miss. Logging in...");

                userKey = await GetUserKeyAsync(apiDevKey, username, password);

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromHours(24));

                _cache.Set(CACHE_KEY, userKey, cacheEntryOptions);
            }
            else
            {
                Console.WriteLine(">>> Cache hit! Reusing Key from RAM.");
            }

            return userKey;
        }
    }
}
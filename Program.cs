using System.Net;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using NobelLaureatesApp.Helper;
using NobelLaureatesApp.Model;

namespace NobelLaureatesApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                string jsonFilePath = "nobel_laureates.json";
                string apiDevKey = "KXaob-uSS0jptuj4BMp6dCW1XHVqkzov";
                string apiUserKey = string.Empty;
                string username = "hung404";
                string password = "Hung404@2620";
                string targetCategory = "medicine";
                int yearLimit = 1980;
                string nobelReportContent = string.Empty;

                var laureates = await GetLaureatesAsync(jsonFilePath, l => l.category == targetCategory && l.year < yearLimit && l.laureates != null);
                Console.WriteLine($"--- List Laureates ({targetCategory} < {yearLimit}) ---");
                foreach (var l in laureates)
                {
                    Console.WriteLine($"{l.firstname} {l.surname} - {l.motivation}");
                    nobelReportContent += $"{l.firstname} {l.surname} ({l.motivation})\n";
                }

                apiUserKey = await PastebinHelper.GetUserKeyWithCacheAsync(apiDevKey, username, password);

                var resultPasteUrl = await PastebinHelper.CreatePastebinAsync(nobelReportContent, apiDevKey, apiUserKey, "Nobel Laureates Report");
                if(!string.IsNullOrEmpty(resultPasteUrl))
                {
                    Console.WriteLine("Created pastebin successfully!");
                }
                
                Console.WriteLine("Do you want to delete the paste? (y/n)");
                string input = Console.ReadLine();
                if(input?.ToLower() == "y")
                {
                    string pasteKey = resultPasteUrl.Split('/').Last();
                    bool deleted = await PastebinHelper.DeletePastebinAsync(pasteKey, apiDevKey, apiUserKey);
                    if (deleted) Console.WriteLine("Deleted Pastebin successfully!");

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        static async Task<List<Laureate>> GetLaureatesAsync(string filePath, Func<Prize, bool> criteria)
        {
            var json = File.ReadAllText(filePath);
            var dataJson = JsonConvert.DeserializeObject<NobelData>(json);
            var filtered = dataJson?.prizes?.Where(criteria)
            .SelectMany(x => x.laureates ?? Enumerable.Empty<Laureate>()).ToList() ?? new List<Laureate>();
            return filtered;
        }
    }
}
using System.Text.Json;

namespace TypeGenerator
{
    internal static class StudioAPI
    {
        private static readonly string _baseURL = "https://raw.githubusercontent.com/MaximumADHD/Roblox-Client-Tracker/roblox/";

        public static async Task<APITypes.Dump> GetDump()
        {
            var body = await Request("Mini-API-Dump.json");
            return JsonSerializer.Deserialize<APITypes.Dump>(body)!;
        }

        public static async Task<ReflectionMetadataReader> GetReflectionMetadata()
        {
            var body = await Request("ReflectionMetadata.xml");
            return new ReflectionMetadataReader(body);
        }

        private static async Task<string> Request(string endpoint)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var response = await client.GetAsync(_baseURL + endpoint);
                    response.EnsureSuccessStatusCode();

                    return await response.Content.ReadAsStringAsync();
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine($"Request error: {e.Message}");
                    Environment.Exit(1);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Unexpected error: {e.Message}");
                    Environment.Exit(1);
                }
            }
            return null!;
        }
    }
}
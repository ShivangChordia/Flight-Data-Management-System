using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Temp_GUI_GroundTerminal
{
    public class TelemetryApiService
    {
        private readonly HttpClient _httpClient;

        public TelemetryApiService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:7224/api/telemetryController")

            };
        }

        // Method to fetch telemetry data based on a search term
        public async Task<List<TelemetryDataModel>> SearchTelemetryAsync(string searchTerm)
        {
            var response = await _httpClient.GetAsync($"https://localhost:7224/api/telemetryController/search?query={searchTerm}");

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var telemetryData = JsonConvert.DeserializeObject<List<TelemetryDataModel>>(jsonResponse);
                return telemetryData; // Return the first item if found
            }

            return null;
        }
    }
}

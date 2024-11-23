/*
* FILE : TelemetryApiService.cs
* PROJECT : SENG3020 - Milestone #2 
* PROGRAMMER : Shivang Chordia, Keval PAtel, Urvish Motivaras & Jaygiri Goswami
* DATE : 2024-11-22
* DESCRIPTION : The TelemetryApiService class is a service designed to interact with a web API endpoint that provides telemetry data.
*/


using System.Net.Http;
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

        /*
        * FUNCTION: SearchTelemetryAsync()
        * DESCRIPTION:  Method to fetch telemetry data based on a search term
        * PARAMETERS: string searchTerm ->  used to query the telemetry data
        * RETURN: Task (Asynchronous)
        */
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

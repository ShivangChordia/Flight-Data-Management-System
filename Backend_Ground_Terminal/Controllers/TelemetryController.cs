/*
* FILE : TelemetryController.cs
* PROJECT : SENG3020 - Milestone #2 
* PROGRAMMER : Shivang Chordia, Keval PAtel, Urvish Motivaras & Jaygiri Goswami
* DATE : 2024-11-22
* DESCRIPTION : This file defines the TelemetryController class, an API controller for managing telemetry data in the Ground Terminal Management System. 
*/


using Microsoft.AspNetCore.Mvc;
using Ground_Terminal_Management_System.Services;
using Backend_Ground_Terminal.Model;

namespace Ground_Terminal_Management_System.Controllers
{
    [Route("api/telemetryController")]
    [ApiController]
    public class TelemetryController : ControllerBase
    {
        private readonly DatabaseService _databaseService;

        /*
        * CONSTRUCTOR: TelemetryController()
        * DESCRIPTION: Constructor injection of DatabaseService
        * PARAMETERS: DatabaseService databaseService
        * RETURN: n/a
        */
        public TelemetryController(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        /*
        * FUNCTION: SearchTelemetryData()
        * DESCRIPTION: The SearchTelemetryData function is an API endpoint that handles HTTP GET requests for searching telemetry data.
        * PARAMETERS: ([FromQuery] string query) -> [FromQuery] string query) which is used to filter or search the telemetry data.
        * RETURN: 200 OK with the search results (list of TelemetryDataModel) if data is found.
                  404 Not Found if no results are found.
                  500 Internal Server Error if there is an exception.
        */
        [HttpGet("search")]
        public ActionResult<List<TelemetryDataModel>> SearchTelemetryData([FromQuery] string query)
        {
            try
            {
                var telemetryData = _databaseService.SearchTelemetryData(query);

                if (telemetryData == null || telemetryData.Count == 0)
                {
                    return NotFound(new { message = "No data found for the given search term." });
                }

                return Ok(telemetryData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error occurred: {ex.Message}" });
            }
        }
    }
}

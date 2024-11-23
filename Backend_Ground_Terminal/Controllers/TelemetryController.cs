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

        // Constructor injection of DatabaseService
        public TelemetryController(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        // GET api/telemetryController/search?query={tailNumber}
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

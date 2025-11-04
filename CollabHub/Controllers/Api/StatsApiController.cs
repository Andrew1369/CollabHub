using CollabHub.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace CollabHub.Controllers.Api
{
    [ApiController]
    [Route("api/v1/stats")]
    public class StatsApiController : ControllerBase
    {
        // GET: /api/v1/stats/events-by-venue
        [HttpGet("events-by-venue")]
        public ActionResult<IEnumerable<StatBucketDto>> GetEventsByVenue()
        {
            // Демонстраційні дані для F1.
            // Можеш перейменувати / підкоригувати назви під свої реальні venues.
            var data = new List<StatBucketDto>
            {
                new StatBucketDto("Main Hall", 5),
                new StatBucketDto("Room 101", 3),
                new StatBucketDto("Conference Center", 2),
                new StatBucketDto("Online", 4)
            };

            return Ok(data);
        }
    }
}

using CodingTest.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CodingTest.Controllers
{
    [ApiController]
    [Route("api/stories")]
    [EnableRateLimiting("fixed")]
    public class StoriesController : ControllerBase
    {

        private readonly IHackerNewsService _hackerNewsService;
        private readonly ILogger<StoriesController> _logger;

        public StoriesController(IHackerNewsService hackerNewsService, ILogger<StoriesController> logger)
        {
            _hackerNewsService = hackerNewsService;
            _logger = logger;
        }

        [HttpGet("best")]
        public async Task<IActionResult> GetBestStories([FromQuery] int n)
        {
            try
            {
                if (n<=0)
                {
                    return BadRequest("Bad request of stories must be greater than 0");

                }
                var stories = await _hackerNewsService.GetBestStoriesAsync(n);
                return Ok(stories);
            }
            catch (Exception ex)
            {

                _logger.LogError("Error retrieving best stories");
                return StatusCode(500,"An error ocurred while processing your request");
            }

        }
        
    }
}

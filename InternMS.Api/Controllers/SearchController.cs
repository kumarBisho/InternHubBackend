using InternMS.Api.DTOs.Search;
using InternMS.Api.Services.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternMS.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/search")]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _searchService;
        private readonly ILogger<SearchController> _logger;

        public SearchController(ISearchService searchService, ILogger<SearchController> logger)
        {
            _searchService = searchService;
            _logger = logger;
        }

        /// <summary>
        /// Search tasks with advanced filters
        /// </summary>
        [HttpPost("tasks")]
        public async Task<IActionResult> SearchTasks([FromBody] TaskSearchRequestDto request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new { message = "Search request cannot be null" });
                }

                var results = await _searchService.SearchTasksAsync(request);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error searching tasks: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while searching tasks" });
            }
        }

        /// <summary>
        /// Search projects with advanced filters
        /// </summary>
        [HttpPost("projects")]
        public async Task<IActionResult> SearchProjects([FromBody] ProjectSearchRequestDto request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new { message = "Search request cannot be null" });
                }

                var results = await _searchService.SearchProjectsAsync(request);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error searching projects: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while searching projects" });
            }
        }

        /// <summary>
        /// Search users with advanced filters
        /// </summary>
        [HttpPost("users")]
        public async Task<IActionResult> SearchUsers([FromBody] UserSearchRequestDto request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new { message = "Search request cannot be null" });
                }

                var results = await _searchService.SearchUsersAsync(request);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error searching users: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while searching users" });
            }
        }

        /// <summary>
        /// Global search across all resource types
        /// </summary>
        [HttpPost("global")]
        public async Task<IActionResult> GlobalSearch([FromBody] GlobalSearchRequestDto request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.SearchQuery))
                {
                    return BadRequest(new { message = "Search query is required" });
                }

                var results = await _searchService.GlobalSearchAsync(request);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in global search: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred during global search" });
            }
        }

        /// <summary>
        /// Quick search with query string
        /// </summary>
        [HttpGet("quick")]
        public async Task<IActionResult> QuickSearch([FromQuery] string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return BadRequest(new { message = "Search query is required" });
                }

                var request = new GlobalSearchRequestDto
                {
                    SearchQuery = query,
                    PageSize = 5
                };

                var results = await _searchService.GlobalSearchAsync(request);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in quick search: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred during quick search" });
            }
        }
    }
}

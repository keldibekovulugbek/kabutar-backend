using Kabutar.Service.DTOs.Search;
using Kabutar.Service.Services.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Kabutar.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SearchController : ControllerBase
{
    private readonly ISearchService _searchService;

    public SearchController(ISearchService searchService)
    {
        _searchService = searchService;
    }

    /// <summary>
    /// Global search - userlar va messagelar bo'yicha qidirish
    /// </summary>
    /// <param name="query">Qidiruv matni</param>
    /// <returns>Userlar va messagelar ro'yxati</returns>
    [HttpGet]
    public async Task<ActionResult<SearchResultDTO>> Search([FromQuery] string query)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out long userId))
        {
            return Unauthorized("User ID not found in token");
        }

        var result = await _searchService.SearchAsync(query, userId);
        return Ok(result);
    }
}

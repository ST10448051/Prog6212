
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/antiforgery")]
public class AntiforgeryController : ControllerBase
{
    private readonly IAntiforgery _antiforgery;

    public AntiforgeryController(IAntiforgery antiforgery)
    {
        _antiforgery = antiforgery;
    }

    [HttpGet("token")]
    [AllowAnonymous]
    public IActionResult GetToken()
    {
        Response.Headers.CacheControl = "no-store";

        var tokens = _antiforgery.GetAndStoreTokens(HttpContext);

        return Ok(new
        {
            token = tokens.RequestToken
        });
    }
}

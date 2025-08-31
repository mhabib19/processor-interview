using CardProcessor.API.DTOs;
using CardProcessor.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace CardProcessor.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TokenController : ControllerBase
{
    private readonly IJwtService _jwtService;

    public TokenController(IJwtService jwtService)
    {
        _jwtService = jwtService;
    }

    [HttpGet]
    public ActionResult<TokenResponse> GetToken()
    {
        try
        {
            // Simple token generation - no user validation needed
            var token = _jwtService.GenerateToken("api-client", "ApiUser");
            
            return Ok(new TokenResponse
            {
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddHours(24) // 24-hour token
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = $"Token generation failed: {ex.Message}" });
        }
    }
}

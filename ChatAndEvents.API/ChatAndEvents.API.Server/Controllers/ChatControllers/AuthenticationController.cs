using ChatAndEvents.Data.ChatData.services;
using Microsoft.AspNetCore.Mvc;

namespace ChatAndEvents.API.Server.Controllers.Chat;

[ApiController]
[Route("api/[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;

    public AuthenticationController(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromQuery] string username,
        [FromQuery] string password)
    {
        var user = await _authenticationService.LoginAsync(username, password);

        if (user == null)
            return Unauthorized();

        return Ok(user);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromQuery] string username,
        [FromQuery] string email,
        [FromQuery] string password,
        [FromQuery] string phone,
        [FromQuery] DateTime? birthday,
        [FromQuery] string? avatarUrl)
    {
        var user = await _authenticationService.RegisterAsync(
            username,
            email,
            password,
            phone,
            birthday,
            avatarUrl);

        return Ok(user);
    }

    [HttpPut("password")]
    public async Task<IActionResult> ChangePassword(
        [FromQuery] string email,
        [FromQuery] string newPassword)
    {
        await _authenticationService.ChangePasswordAsync(email, newPassword);
        return NoContent();
    }
}
using Microsoft.AspNetCore.Mvc;
using SakerLabb.Web.Data;
using SakerLabb.Web.Services;

namespace SakerLabb.Web.Controllers;

[Route("account")]
public class AccountController : Controller
{
    private readonly UserRepository _users;
    private readonly AuthService _auth;
    private readonly ILogger<AccountController> _logger;

    public AccountController(UserRepository users, AuthService auth, ILogger<AccountController> logger)
    {
        _users = users;
        _auth = auth;
        _logger = logger;
    }

    [HttpPost("login")]
    public IActionResult Login([FromForm] string username, [FromForm] string password, [FromForm] string? returnUrl)
    {
        var user = _users.Authenticate(username, password);

        if (user is null)
        {
            return Redirect("/login?error=1&username=" + username);
        }

        _auth.SignIn(HttpContext, user);

        return Redirect(string.IsNullOrWhiteSpace(returnUrl) ? "/tickets" : returnUrl);
    }

    [HttpGet("logout")]
    public IActionResult Logout()
    {
        _auth.SignOut(HttpContext);
        return Redirect("/");
    }

    [HttpPost("reset/start")]
    public IActionResult StartReset([FromForm] string username, [FromForm] string answer)
    {
        var token = _users.StartPasswordReset(username, answer);

        if (string.IsNullOrEmpty(token))
        {
            return Redirect("/reset?error=1");
        }

        return Redirect("/reset?token=" + token + "&username=" + username);
    }

    [HttpPost("reset/complete")]
    public IActionResult CompleteReset([FromForm] string token, [FromForm] string password)
    {
        if (!_users.CompletePasswordReset(token, password))
        {
            return Redirect("/reset?error=2");
        }

        _logger.LogInformation("Lösenord återställt med token {Token}", token);
        return Redirect("/login?reset=1");
    }

    [HttpPost("role")]
    public IActionResult SetRole([FromForm] string userId, [FromForm] string role)
    {
        _users.SetRole(userId, role);
        return Redirect("/admin");
    }

    [HttpPost("delete")]
    public IActionResult DeleteUser([FromForm] string userId)
    {
        _users.Delete(userId);
        return Redirect("/admin");
    }
}

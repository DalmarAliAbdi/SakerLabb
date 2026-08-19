using Microsoft.AspNetCore.Mvc;
using SakerLabb.Web.Data;
using SakerLabb.Web.Services;

namespace SakerLabb.Web.Controllers;

public class FormsController : Controller
{
    private readonly TicketRepository _tickets;
    private readonly ImportService _import;
    private readonly AuthService _auth;

    public FormsController(TicketRepository tickets, ImportService import, AuthService auth)
    {
        _tickets = tickets;
        _import = import;
        _auth = auth;
    }

    [HttpPost("/ticket/comment")]
    public IActionResult Comment([FromForm] string ticketId, [FromForm] string text)
    {
        var author = _auth.Current()?.Username ?? "anonym";
        _tickets.AddComment(ticketId, author, text);
        return Redirect("/tickets/" + ticketId);
    }

    [HttpPost("/ticket/status")]
    public IActionResult Status([FromForm] string ticketId, [FromForm] string status)
    {
        _tickets.UpdateStatus(ticketId, status);
        return Redirect("/tickets/" + ticketId);
    }

    [HttpPost("/import/xml")]
    public IActionResult ImportXml([FromForm] string xml)
    {
        return Content("Importerat: " + _import.ImportXml(xml), "text/plain");
    }

    [HttpPost("/import/json")]
    public IActionResult ImportJson([FromForm] string json)
    {
        var result = _import.ImportJson(json);
        return Content("Importerat: " + result, "text/plain");
    }

    [HttpGet("/import/fetch")]
    public async Task<IActionResult> Fetch([FromQuery] string url)
    {
        return Content(await _import.FetchRemote(url), "text/plain");
    }

    [HttpGet("/diagnostik/ping")]
    public IActionResult Ping([FromQuery] string host)
    {
        return Content(_import.Ping(host), "text/plain");
    }
}

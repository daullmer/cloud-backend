using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CloudComputingBackend.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class AuthTestController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return NoContent();
    }
}
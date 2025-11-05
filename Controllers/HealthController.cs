using Microsoft.AspNetCore.Mvc;

namespace kv_be_csharp_dotnet_dataapi_collections.Controllers;

[ApiController]
[Route("/api/v1/health")]
[Produces("application/json")]
public class HealthController : Controller
{
    [HttpGet]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public IActionResult GetHealth()
    {
        return Ok("Service is up and running!");
    }
}
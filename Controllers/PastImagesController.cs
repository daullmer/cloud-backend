using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CloudComputingBackend.Controllers;

[ApiController]
[Route("[controller]")]
public class PastImagesController : Controller
{
    private readonly string _imageBaseFolderPath;
    private readonly ILogger<PastImagesController> _logger;

    public PastImagesController(ILogger<PastImagesController> logger, IConfiguration config)
    {
        _logger = logger;
        _imageBaseFolderPath = Path.Combine(Directory.GetCurrentDirectory(),
            config.GetValue<string>("ImageFolderName"));
        if (!Directory.Exists(_imageBaseFolderPath)) Directory.CreateDirectory(_imageBaseFolderPath);
    }

    [Authorize]
    [HttpGet(Name = "GetPastImages")]
    public IEnumerable<Guid> GetPastImages()
    {
        _logger.LogInformation("Getting list of past image GUIDs");
        var dirs = Directory.GetDirectories(_imageBaseFolderPath);
        var guids = dirs.Select(x =>
        {
            var parsable = Guid.TryParse(Path.GetFileName(x), out Guid guid);
            if (parsable) return guid;
            return Guid.Empty;
        })
            .Where(x => x != Guid.Empty)
            .ToList();
        return guids;
    }

    [HttpGet("{guid}/Image")]
    public IActionResult GetPastImage(Guid guid)
    {
        _logger.LogInformation("Getting image with GUID {Guid}", guid);
        var image = Path.Combine(_imageBaseFolderPath, guid.ToString(), "image.jpg");
        return PhysicalFile(image, MediaTypeNames.Image.Jpeg);
    }
    
    [Authorize]
    [HttpGet("{guid}/Vision")]
    public IActionResult GetPastVisionResult(Guid guid)
    {
        _logger.LogInformation("Getting vision results for image with GUID {Guid}", guid);
        var image = Path.Combine(_imageBaseFolderPath, guid.ToString(), "result.json");
        return PhysicalFile(image, MediaTypeNames.Application.Json);
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace CloudComputingBackend.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class AnalyzeController : Controller
{
    private readonly ILogger _logger;
    private readonly string _imageBaseFolderPath;
    private readonly IFaceClient _faceClient;

    public AnalyzeController(IConfiguration config, ILogger<AnalyzeController> logger)
    {
        _logger = logger;
        _imageBaseFolderPath = Path.Combine(Directory.GetCurrentDirectory(),
            config.GetValue<string>("ImageFolderName"));
        if (!Directory.Exists(_imageBaseFolderPath)) Directory.CreateDirectory(_imageBaseFolderPath);
        
        // authenticate face client
        _faceClient = new FaceClient(new ApiKeyServiceClientCredentials("9b255ab611dd467c863ecc36a2036125")) { Endpoint = "https://cloud-computing-vision.cognitiveservices.azure.com" };
        
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<IEnumerable<DetectedFace>>> AnalyzeImage()
    {        
        // read body to stream
        await using var imageStream = await GetMemoryStream(Request.Body);
        var copyStream = new MemoryStream();
        await imageStream.CopyToAsync(copyStream);
        imageStream.Position = 0;
        copyStream.Position = 0;

        // call azure with image
        var faceAttributes = new List<FaceAttributeType>
        {
            FaceAttributeType.Accessories,
            FaceAttributeType.Age,
            FaceAttributeType.Emotion,
            FaceAttributeType.Gender,
            FaceAttributeType.Gender,
            FaceAttributeType.FacialHair,
            FaceAttributeType.Smile,
            FaceAttributeType.Hair,
            FaceAttributeType.Makeup,
            FaceAttributeType.Glasses,
            FaceAttributeType.HeadPose
        };
        var detectedFaces = await _faceClient.Face.DetectWithStreamAsync(imageStream, detectionModel: "detection_01",
            returnFaceAttributes: faceAttributes);

        if (detectedFaces.Count < 1) {
            return Ok(detectedFaces);
        }

        var imageGuid = Guid.NewGuid();
        Response.Headers.Add("BackendGUID", imageGuid.ToString());
        _logger.LogInformation("Analyzing new Image. Will be getting Guid {Guid}", imageGuid);

        // save image to directory
        var folderPath = Path.Combine(_imageBaseFolderPath, imageGuid.ToString());
        var imagePath = Path.Combine(folderPath, "image.jpg");
        var jsonPath = Path.Combine(folderPath, "result.json");
        Directory.CreateDirectory(folderPath);

        await using var fs = new FileStream(imagePath, FileMode.Create, FileAccess.Write);
        await copyStream.CopyToAsync(fs);
        
        // Save result to directory
        await using var jsonStream = new FileStream(jsonPath, FileMode.Create, FileAccess.Write);
        await JsonSerializer.SerializeAsync(jsonStream, detectedFaces, typeof(IList<DetectedFace>));

        return Ok(detectedFaces);
    }
    
    private static async Task<MemoryStream> GetMemoryStream(Stream inputStream)
    {
        using var reader = new StreamReader(inputStream);
        var text = await reader.ReadToEndAsync().ConfigureAwait(false);

        var bytes = Convert.FromBase64CharArray(text.ToCharArray(), 0, text.Length);
        return new MemoryStream(bytes, 0, bytes.Length, false, true);
    }
    
}

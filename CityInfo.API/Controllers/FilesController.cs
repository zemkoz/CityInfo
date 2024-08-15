using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace CityInfo.API.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Authorize]
[Route("api/v{version:apiVersion}/files")]
public class FilesController : Controller
{
    private readonly FileExtensionContentTypeProvider _contentTypeProvider;

    public FilesController(FileExtensionContentTypeProvider contentTypeProvider)
    {
        _contentTypeProvider = contentTypeProvider;
    }

    [HttpGet("{fileId}")]
    [ApiVersion(0.1, Deprecated = true)]
    public IActionResult GetFile(string fileId)
    {
        var pathToFile = $"uploaded_file_{fileId}.pdf";
        if (!System.IO.File.Exists(pathToFile))
        {
            return NotFound();
        }

        if (!_contentTypeProvider.TryGetContentType(pathToFile, out var contentType))
        {
            contentType = "application/octet-stream";
        }
        
        var bytes = System.IO.File.ReadAllBytes(pathToFile);
        return File(bytes, contentType, Path.GetFileName(pathToFile));
    }

    [HttpPost]
    public async Task<ActionResult> CreateFile(IFormFile file)
    {
        if (file.Length == 0 || file.Length > 20971520 || file.ContentType != "application/pdf")
        {
            return BadRequest();
        }
        
        var fileId = Guid.NewGuid();
        var path = Path.Combine(Directory.GetCurrentDirectory(), $"uploaded_file_{fileId}.pdf");
        await using (var stream = new FileStream(path, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }
        
        return Ok(new
        {
            id=fileId
        });
    }
}
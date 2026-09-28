using System.Text.Json;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using EJ2FileManagerProviders.Providers;
using Syncfusion.EJ2.FileManager.Base;

namespace EJ2FileManagerProviders.Controllers;

[Route("api/[controller]")]
[EnableCors("AllowAllOrigins")]
[ApiController]
public class FileManagerController : ControllerBase
{
    private readonly IFileManagerProvider _provider;

    public FileManagerController(IFileManagerProvider provider)
    {
        _provider = provider;
    }

    [HttpPost("FileOperations")]
    public object? FileOperations([FromBody] FileManagerDirectoryContent args)
        => _provider.FileOperations(args);

    [HttpPost("Upload")]
    [DisableRequestSizeLimit]
    public IActionResult Upload(
        [FromForm] string? path,
        [FromForm] long size,
        [FromForm] IList<IFormFile> uploadFiles,
        [FromForm] string? action,
        [FromForm] string? data)
        => _provider.Upload(HttpContext, path, size, uploadFiles, action, data);

    [HttpPost("Download")]
    public IActionResult Download([FromForm(Name = "downloadInput")] string? downloadInput)
    {
        var args = string.IsNullOrWhiteSpace(downloadInput)
            ? new FileManagerDirectoryContent()
            : JsonSerializer.Deserialize<FileManagerDirectoryContent>(downloadInput,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new FileManagerDirectoryContent();

        return _provider.Download(args);
    }

    [HttpGet("GetImage")]
    public IActionResult GetImage([FromQuery] FileManagerDirectoryContent args)
        => _provider.GetImage(args);
}

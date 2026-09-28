using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Syncfusion.EJ2.FileManager.Base;

namespace EJ2FileManagerProviders.Providers;

public interface IFileManagerProvider
{
    object? FileOperations(object args);
    IActionResult Upload(HttpContext context, string? path, long size, IList<IFormFile> uploadFiles, string? action, string? data);
    IActionResult Download(FileManagerDirectoryContent args);
    IActionResult GetImage(FileManagerDirectoryContent args);
}
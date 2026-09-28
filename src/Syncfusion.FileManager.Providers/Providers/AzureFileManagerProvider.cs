using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Syncfusion.FileManager.AzureFileProvider;
using AzureBase = Syncfusion.FileManager.AzureFileProvider.Base;

namespace EJ2FileManagerProviders.Providers;

public sealed class AzureFileManagerProvider : IFileManagerProvider
{
    private readonly AzureFileProvider _operation = new();
    private readonly string _blobPath;
    private readonly string _filePath;

    public AzureFileManagerProvider()
    {
        var accountName = Environment.GetEnvironmentVariable("AZURE_ACCOUNT_NAME") ?? string.Empty;
        var accountKey = Environment.GetEnvironmentVariable("AZURE_ACCOUNT_KEY") ?? string.Empty;
        var blobName = Environment.GetEnvironmentVariable("AZURE_BLOB_NAME") ?? string.Empty;
        var blobPath = Environment.GetEnvironmentVariable("AZURE_BLOB_PATH") ?? string.Empty;
        var filePath = Environment.GetEnvironmentVariable("AZURE_FILE_PATH") ?? string.Empty;
        if (string.IsNullOrWhiteSpace(accountName) || string.IsNullOrWhiteSpace(accountKey) || string.IsNullOrWhiteSpace(blobName))
            throw new InvalidOperationException("Azure account name, key, and blob name are required.");

        _blobPath = NormalizeBlobPath(blobPath);
        _filePath = NormalizeFilePath(filePath);
        _operation.SetBlobContainer(_blobPath, _filePath);
        _operation.RegisterAzure(accountName, accountKey, blobName);
    }

    public object? FileOperations(object args)
    {
        var content = Deserialize<AzureBase.FileManagerDirectoryContent>(args);
        _operation.SetRules(GetRules());
        NormalizeRequestPaths(content);

        return content.Action switch
        {
            "read" => _operation.ToCamelCase(_operation.GetFiles(content.Path, content.ShowHiddenItems, content.Data)),
            "delete" => _operation.ToCamelCase(_operation.Delete(content.Path, content.Names, content.Data)),
            "details" => _operation.ToCamelCase(_operation.Details(content.Path, content.Names, content.Data)),
            "create" => _operation.ToCamelCase(_operation.Create(content.Path, content.Name, content.Data)),
            "search" => _operation.ToCamelCase(_operation.Search(content.Path, content.SearchString, content.ShowHiddenItems, content.CaseSensitive, content.Data)),
            "rename" => _operation.ToCamelCase(_operation.Rename(content.Path, content.Name, content.NewName, false, content.ShowFileExtension, content.Data)),
            "copy" => _operation.ToCamelCase(_operation.Copy(content.Path, content.TargetPath, content.Names, content.RenameFiles, content.TargetData, content.Data)),
            "move" => _operation.ToCamelCase(_operation.Move(content.Path, content.TargetPath, content.Names, content.RenameFiles, content.TargetData, content.Data)),
            _ => null
        };
    }

    public IActionResult Upload(HttpContext context, string? path, long size, IList<IFormFile> uploadFiles, string? action, string? data)
    {
        var content = new AzureBase.FileManagerDirectoryContent
        {
            Path = path ?? string.Empty,
            Action = action ?? string.Empty,
            UploadFiles = uploadFiles,
            Data = DeserializeData(data ?? string.Empty)
        };
        if (!string.IsNullOrEmpty(content.Path))
        {
            var originalPath = _filePath.Replace(_blobPath, "");
            content.Path = (originalPath + content.Path).Replace("//", "/");
        }
        var chunkIndex = int.TryParse(context.Request.Form["chunk-index"], out var chunk) ? chunk : 0;
        var totalChunk = int.TryParse(context.Request.Form["total-chunk"], out var total) ? total : 0;
        var uploadResponse = _operation.Upload(content.Path, content.UploadFiles, content.Action, chunkIndex, totalChunk, content.Data);
        if (uploadResponse.Error != null)
        {
            context.Response.Clear();
            context.Response.ContentType = "application/json; charset=utf-8";
            context.Response.StatusCode = Convert.ToInt32(uploadResponse.Error.Code);
            context.Features.Get<Microsoft.AspNetCore.Http.Features.IHttpResponseFeature>()!.ReasonPhrase = uploadResponse.Error.Message;
        }
        return new JsonResult("");
    }

    public IActionResult Download(Syncfusion.EJ2.FileManager.Base.FileManagerDirectoryContent args)
    {
        var content = Deserialize<AzureBase.FileManagerDirectoryContent>(args);
        return _operation.Download(content.Path, content.Names, content.Data);
    }

    public IActionResult GetImage(Syncfusion.EJ2.FileManager.Base.FileManagerDirectoryContent args)
    {
        var data = args.Data == null
            ? Array.Empty<AzureBase.FileManagerDirectoryContent>()
            : DeserializeData(JsonSerializer.Serialize(args.Data, ProviderJson.Options));
        return _operation.GetImage(args.Path, args.Id, true, null, data);
    }

    private void NormalizeRequestPaths(AzureBase.FileManagerDirectoryContent content)
    {
        if (string.IsNullOrEmpty(content.Path)) return;
        var originalPath = _filePath.Replace(_blobPath, "");
        content.Path = !content.Path.Contains(originalPath) ? (originalPath + content.Path).Replace("//", "/") : content.Path.Replace("//", "/");
        content.TargetPath = (originalPath + content.TargetPath).Replace("//", "/");
    }

    private static AzureBase.FileManagerDirectoryContent[] DeserializeData(string data)
    {
        if (string.IsNullOrWhiteSpace(data)) return new[] { new AzureBase.FileManagerDirectoryContent() };
        using var document = JsonDocument.Parse(data);
        return document.RootElement.ValueKind == JsonValueKind.Array
            ? JsonSerializer.Deserialize<AzureBase.FileManagerDirectoryContent[]>(data, ProviderJson.Options) ?? Array.Empty<AzureBase.FileManagerDirectoryContent>()
            : new[] { JsonSerializer.Deserialize<AzureBase.FileManagerDirectoryContent>(data, ProviderJson.Options) ?? new AzureBase.FileManagerDirectoryContent() };
    }

    private static T Deserialize<T>(object value) where T : new()
    {
        var json = value switch
        {
            JsonElement element => element.GetRawText(),
            string text => text,
            _ => JsonSerializer.Serialize(value, ProviderJson.Options)
        };
        return string.IsNullOrWhiteSpace(json) ? new T() : JsonSerializer.Deserialize<T>(json, ProviderJson.Options) ?? new T();
    }

    private static string NormalizeBlobPath(string path) => path.Replace("../", "").TrimEnd('/', '\\') + "/";
    private static string NormalizeFilePath(string path) => path.Replace("../", "").TrimEnd('/', '\\');

    private static AzureBase.AccessDetails GetRules() => new()
    {
        AccessRules = new List<AzureBase.AccessRule>(),
        Role = "Document Manager"
    };
}
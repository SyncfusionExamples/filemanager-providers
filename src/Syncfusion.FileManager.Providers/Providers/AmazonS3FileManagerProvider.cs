using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Syncfusion.EJ2.FileManager.AmazonS3FileProvider;
using AmazonBase = Syncfusion.EJ2.FileManager.Base;

namespace EJ2FileManagerProviders.Providers;

public sealed class AmazonS3FileManagerProvider : IFileManagerProvider
{
    private readonly AmazonS3FileProvider _operation;

    public AmazonS3FileManagerProvider()
    {
        var bucketName = Environment.GetEnvironmentVariable("AWS_BUCKET_NAME") ?? string.Empty;
        var accessKeyId = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID") ?? string.Empty;
        var secretAccessKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY") ?? string.Empty;
        var region = Environment.GetEnvironmentVariable("AWS_BUCKET_REGION") ?? string.Empty;
        if (string.IsNullOrWhiteSpace(bucketName) || string.IsNullOrWhiteSpace(accessKeyId) || string.IsNullOrWhiteSpace(secretAccessKey) || string.IsNullOrWhiteSpace(region))
            throw new InvalidOperationException("AWS bucket, access key, secret key, and region are required.");

        // The SDK provider creates its TransferUtility before RegisterAmazonS3 sets
        // the static client. Register once, then create the active model so it
        // captures the initialized client just like the standalone controller.
        var registrationOperation = new AmazonS3FileProvider();
        registrationOperation.RegisterAmazonS3(bucketName, accessKeyId, secretAccessKey, region);
        _operation = new AmazonS3FileProvider();
        _operation.RegisterAmazonS3(bucketName, accessKeyId, secretAccessKey, region);
    }

    public object? FileOperations(object args)
    {
        var content = Deserialize<AmazonBase.FileManagerDirectoryContent>(args);
        _operation.SetRules(GetRules());
        if ((content.Action == "delete" || content.Action == "rename") && content.TargetPath == null && content.Path == "")
            return _operation.ToCamelCase(new AmazonBase.FileManagerResponse { Error = new AmazonBase.ErrorDetails { Code = "401", Message = "Restricted to modify the root folder." } });

        return content.Action switch
        {
            "read" => _operation.ToCamelCase(_operation.GetFiles(content.Path, false, content.Data)),
            "delete" => _operation.ToCamelCase(_operation.Delete(content.Path, content.Names, content.Data)),
            "copy" => _operation.ToCamelCase(_operation.Copy(content.Path, content.TargetPath, content.Names, content.RenameFiles, content.TargetData, content.Data)),
            "move" => _operation.ToCamelCase(_operation.Move(content.Path, content.TargetPath, content.Names, content.RenameFiles, content.TargetData, content.Data)),
            "details" => _operation.ToCamelCase(_operation.Details(content.Path, content.Names, content.Data)),
            "create" => _operation.ToCamelCase(_operation.Create(content.Path, content.Name, content.Data)),
            "search" => _operation.ToCamelCase(_operation.Search(content.Path, content.SearchString, content.ShowHiddenItems, content.CaseSensitive, content.Data)),
            "rename" => _operation.ToCamelCase(_operation.Rename(content.Path, content.Name, content.NewName, false, content.ShowFileExtension, content.Data)),
            _ => null
        };
    }

    public IActionResult Upload(HttpContext context, string? path, long size, IList<IFormFile> uploadFiles, string? action, string? data)
    {
        path ??= string.Empty;
        action ??= string.Empty;
        uploadFiles ??= Array.Empty<IFormFile>();
        var parsed = Deserialize<AmazonBase.FileManagerDirectoryContent>(data ?? string.Empty);
        var dataObject = new[] { parsed };
        foreach (var file in uploadFiles)
        {
            var folders = file.FileName.Split('/');
            if (folders.Length > 1)
            {
                for (var index = 0; index < folders.Length - 1; index++)
                {
                    if (!_operation.checkFileExist(path, folders[index]))
                    {
                        _operation.ToCamelCase(_operation.Create(path, folders[index], dataObject));
                    }
                    path += folders[index] + "/";
                }
            }
        }
        var chunkIndex = int.TryParse(context.Request.Form["chunk-index"], out var chunk) ? chunk : 0;
        var totalChunk = int.TryParse(context.Request.Form["total-chunk"], out var total) ? total : 0;
        var uploadResponse = _operation.Upload(path, uploadFiles, action, dataObject, chunkIndex, totalChunk);
        if (uploadResponse.Error != null)
        {
            context.Response.Clear();
            context.Response.ContentType = "application/json; charset=utf-8";
            context.Response.StatusCode = Convert.ToInt32(uploadResponse.Error.Code);
            context.Features.Get<Microsoft.AspNetCore.Http.Features.IHttpResponseFeature>()!.ReasonPhrase = uploadResponse.Error.Message;
        }
        return new ContentResult
        {
            Content = "",
            ContentType = "text/plain; charset=utf-8"
        };
    }

    public IActionResult Download(Syncfusion.EJ2.FileManager.Base.FileManagerDirectoryContent args)
    {
        var content = Deserialize<AmazonBase.FileManagerDirectoryContent>(args);
        return _operation.Download(content.Path, content.Names);
    }

    public IActionResult GetImage(Syncfusion.EJ2.FileManager.Base.FileManagerDirectoryContent args)
    {
        return _operation.GetImage(args.Path, args.Id, false, null, args.Data);
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

    private static AmazonBase.AccessDetails GetRules() => new()
    {
        AccessRules = new List<AmazonBase.AccessRule>
        {
            new()
            {
                Path = "Pictures/Employees/Adam.png",
                Role = "Document Manager",
                Read = AmazonBase.Permission.Allow,
                Write = AmazonBase.Permission.Deny,
                Copy = AmazonBase.Permission.Deny,
                Download = AmazonBase.Permission.Deny,
                IsFile = true
            },
            new()
            {
                Path = "Music/",
                Role = "Document Manager",
                Write = AmazonBase.Permission.Deny,
                WriteContents = AmazonBase.Permission.Deny,
                Upload = AmazonBase.Permission.Allow,
                UploadContentFilter = AmazonBase.UploadContentFilter.FoldersOnly,
                IsFile = false
            }
        },
        Role = "Document Manager"
    };
}
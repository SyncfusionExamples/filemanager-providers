using EJ2FileManagerProviders.Models;

namespace EJ2FileManagerProviders.Providers;

public sealed class FileManagerProviderResolver
{
    public IFileManagerProvider Current { get; }

    public FileManagerProviderResolver(
        FileManagerProviderOptions options,
        IServiceProvider services)
    {
        Current = options.Provider.Trim().ToLowerInvariant() switch
        {
            "azure" => services.GetRequiredService<AzureFileManagerProvider>(),
            "amazon-s3" => services.GetRequiredService<AmazonS3FileManagerProvider>(),
            _ => throw new InvalidOperationException($"Unsupported File Manager provider '{options.Provider}'.")
        };
    }
}
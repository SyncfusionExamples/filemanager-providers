using System.Text.Json;

namespace EJ2FileManagerProviders.Providers;

internal static class ProviderJson
{
    internal static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

}
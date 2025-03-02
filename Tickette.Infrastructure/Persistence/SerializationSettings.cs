using System.Text.Json;

namespace Tickette.Infrastructure.Persistence;

public static class SerializationSettings
{
    public static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // Converts to lowercase with camelCase
        IncludeFields = true
    };
}

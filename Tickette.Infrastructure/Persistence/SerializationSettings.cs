﻿using System.Text.Json;

namespace Tickette.Infrastructure.Persistence;

public static class SerializationSettings
{
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };
}

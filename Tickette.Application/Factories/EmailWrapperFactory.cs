using System.Text.Json;
using Tickette.Application.Common.Models.Email;

namespace Tickette.Application.Factories;

public static class EmailWrapperFactory
{
    public static EmailWrapper Create<T>(string type, T payload)
    {
        var json = JsonSerializer.Serialize(payload);
        using var doc = JsonDocument.Parse(json);
        return new EmailWrapper(type, doc.RootElement.Clone());
    }
}
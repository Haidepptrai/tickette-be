using System.Text.Json;

namespace Tickette.Application.Common.Models.Email;

public record EmailWrapper
{
    public string Type { get; private set; }
    public JsonElement Payload { get; private set; }

    public EmailWrapper(string type, JsonElement payload)
    {
        Type = type;
        Payload = payload;
    }
}
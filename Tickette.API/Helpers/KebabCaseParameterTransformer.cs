using static System.Text.RegularExpressions.Regex;

namespace Tickette.API.Helpers;

public class KebabCaseParameterTransformer : IOutboundParameterTransformer
{
    public string? TransformOutbound(object? value)
    {
        return value == null ? null :
            // Replace uppercase letters with `-` followed by the lowercase equivalent
            Replace(value.ToString()!, "([a-z])([A-Z])", "$1-$2").ToLower();
    }
}

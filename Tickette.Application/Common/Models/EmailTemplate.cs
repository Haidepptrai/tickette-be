namespace Tickette.Application.Common.Models;

public abstract class EmailTemplateModel
{
    public string Header { get; set; } = "{{Header}}";
    public string Footer { get; set; } = "{{Footer}}";
    public string UnsubscribeLink { get; set; }
}
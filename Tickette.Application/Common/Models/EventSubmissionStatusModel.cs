namespace Tickette.Application.Common.Models;

public class EventSubmissionStatusModel : EmailTemplateModel
{
    public string UserName { get; set; }
    public string EventName { get; set; }
    public string EventStatus { get; set; }
    public string StatusClass { get; set; }
    public string EventStatusLink { get; set; }
}
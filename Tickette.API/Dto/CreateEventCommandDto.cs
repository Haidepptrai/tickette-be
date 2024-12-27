using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using Tickette.Application.Features.Events.Common;

namespace Tickette.API.Dto;

public class CreateEventCommandDto
{
    [FromForm, JsonPropertyName("name")]
    public string Name { get; set; }

    [FromForm, JsonPropertyName("address")]
    public string Address { get; set; }

    [FromForm, JsonPropertyName("category_id")]
    public Guid CategoryId { get; set; }

    [FromForm, JsonPropertyName("description")]
    public string Description { get; set; }

    [FromForm, JsonPropertyName("start_date")]
    public DateTime StartDate { get; set; }

    [FromForm, JsonPropertyName("end_date")]
    public DateTime EndDate { get; set; }

    [FromForm, JsonPropertyName("committee_information")]
    public CommitteeInformation Committee { get; set; }

    [FromForm, JsonPropertyName("ticket_information")]
    public required TicketInformation[] TicketInformation { get; set; }

    [FromForm, JsonPropertyName("logo_file")]
    public IFormFile LogoFile { get; set; }

    [FromForm, JsonPropertyName("banner_file")]
    public IFormFile BannerFile { get; set; }
}

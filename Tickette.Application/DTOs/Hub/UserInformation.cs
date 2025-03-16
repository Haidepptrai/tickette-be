namespace Tickette.Application.DTOs.Hub;

public record UserInformation
{
    public string UserId { get; init; }
    public string UserName { get; init; }
    public string UserConnectionString { get; init; }

    public UserInformation(string userId, string userName, string userConnectionString)
    {
        UserId = userId;
        UserName = userName;
        UserConnectionString = userConnectionString;
    }
}
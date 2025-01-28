namespace Tickette.Application.DTOs.Auth;

public sealed class GoogleUserRequest
{
    public string Email { get; private set; }
    public string Name { get; private set; }
    public string Image { get; private set; }
    public string Provider { get; private set; } // Optional: This could be expanded for other providers

    public GoogleUserRequest(string email, string name, string image, string provider)
    {
        Email = email;
        Name = name;
        Image = image;
        Provider = provider;
    }
}

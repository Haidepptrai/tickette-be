namespace Tickette.Domain.Common;

public static class Constant
{
    public static IReadOnlyDictionary<Guid, string> APPLICATION_ROLES
        => new Dictionary<Guid, string>
        {
            {Guid.Parse("e6f8d674-a563-42dd-8451-32acf2b7cf09"), "Admin" },
            {Guid.Parse("7f6797a6-fbaa-4bc1-a93b-2f492161e734"),  "User" },
            { Guid.Parse("d43610b7-5afd-47a2-b342-d032c2ae0047"), "Moderator" }
        };

    public static string LOCAL_CONNECTION_STRING =
        "Host=localhost;Port=5432;Database=tickette-db;Username=HaiNguyen;Password=root";

    public static string LOCAL_CONNECTRING_STRING_AI =
        "Host=localhost;Port=5433;Database=ai_training;Username=ai_user;Password=ai_password";

    public static class APPLICATION_ROLE
    {
        public const string Admin = "Admin";
        public const string User = "User";
        public const string Moderator = "Moderator";
        public const string Agent = "Agent";
    }
}
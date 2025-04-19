namespace Tickette.Domain.Common;

public static class Constant
{
    public static IReadOnlyList<string> APPLICATION_ROLES => new List<string> { "Admin", "User", "Manager", "Moderator", "Agent" };

    public static string LOCAL_CONNECTION_STRING =
        "Host=localhost;Port=5432;Database=tickette-db;Username=HaiNguyen;Password=root";

    public static string LOCAL_CONNECTRING_STRING_AI =
        "Host=localhost;Port=5433;Database=ai_training;Username=ai_user;Password=ai_password";

    public static class COMMITTEE_MEMBER_ROLES
    {
        public const string EventOwner = "EventOwner";
        public const string Admin = "Admin";
        public const string Manager = "Manager";
        public const string CheckInStaff = "CheckInStaff";
        public const string CheckOutStaff = "CheckOutStaff";
        public const string RedeemStaff = "RedeemStaff";
    }

    public static class APPLICATION_ROLE
    {
        public const string Admin = "Admin";
        public const string User = "User";
        public const string Manager = "Manager";
        public const string Moderator = "Moderator";
    }
}
namespace Tickette.Domain.Common;

public static class Constant
{
    public static IReadOnlyList<string> APPLICATION_ROLES => new List<string> { "Admin", "User", "Manager", "Moderator", "Agent" };

    public static string LOCAL_CONNECTION_STRING =
        "Host=localhost;Port=5432;Database=tickette-db;Username=HaiNguyen;Password=root";

    public static class COMMITTEE_MEMBER_ROLES
    {
        public const string EventOwner = "EventOwner";
        public const string Admin = "Admin";
        public const string Manager = "Manager";
        public const string CheckInStaff = "CheckInStaff";
        public const string CheckOutStaff = "CheckOutStaff";
        public const string RedeemStaff = "RedeemStaff";
    }

    public static class COMMITTEE_MEMBER_ROLES_PERMISSIONS
    {
        public const string Marketing = "Marketing";
        public const string Orders = "Orders";
        public const string SeatMap = "SeatMap";
        public const string Members = "Members";
        public const string CheckIn = "CheckIn";
        public const string CheckOut = "CheckOut";
        public const string Redeem = "Redeem";
        public const string Edit = "Edit";
        public const string Summary = "Summary";
        public const string Voucher = "Voucher";
    }

    public static class APPLICATION_ROLE
    {
        public const string Admin = "Admin";
        public const string User = "User";
        public const string Manager = "Manager";
        public const string Moderator = "Moderator";
    }

    public static string TICKET_RESERVATION_QUEUE = "reverse_ticket";
}
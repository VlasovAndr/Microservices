namespace WebApp.Utility;

public class SD
{
    public static string CouponAPIBaseUrl { get; set; }
    public static string AuthAPIBaseUrl { get; set; }
    public static string ProductAPIBaseUrl { get; set; }
    public static string ShoppingCartAPIBaseUrl { get; set; }
    public static string OrderAPIBaseUrl { get; set; }
	public const string RoleAdmin = "ADMIN";
    public const string RoleCustomer = "CUSTOMER";
    public const string TokenCookie = "JWTToken";

    public enum ApiType
    {
        GET,
        POST,
        PUT,
        DELETE
    }

    public const string Status_Pending = "Pending";
    public const string Status_Approved = "Approved";
    public const string Status_ReadyForPickup = "ReadyForPickup";
    public const string Status_Completed = "Completed";
    public const string Status_Refunded = "Refunded";
    public const string Status_Cancelled = "Cancelled";
}

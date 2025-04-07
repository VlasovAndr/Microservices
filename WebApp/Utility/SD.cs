namespace WebApp.Utility;

public class SD
{
    public static string CouponAPI { get; set; }
    public static string AuthAPI { get; set; }
    public static string ProductAPI { get; set; }
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
}

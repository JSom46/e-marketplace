namespace Configuration;

public class JwtBearerConfiguration
{
    public string SecretKey { get; set; }
    public string Audience { get; set; }
    public string Issuer { get; set; }
    public int ExpiryTimeInSeconds { get; set; }
}
namespace ShiftEngine.Api.Configuration;

public sealed class JwtOptions
{
    public string Key { get; set; } = "";
    public string Issuer { get; set; } = "ShiftEngine";
    public string Audience { get; set; } = "ShiftEngineClients";
}

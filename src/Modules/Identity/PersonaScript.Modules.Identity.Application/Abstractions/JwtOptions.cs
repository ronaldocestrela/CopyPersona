namespace PersonaScript.Modules.Identity.Application.Abstractions;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "PersonaScript";

    public string Audience { get; set; } = "PersonaScriptApp";

    public string Secret { get; set; } = "PersonaScript_Super_Secret_Jwt_Signing_Key_2026_Min_256_Bits!";

    public int ExpirationMinutes { get; set; } = 120;
}

using System.IdentityModel.Tokens.Jwt;
using FluentAssertions;
using Microsoft.Extensions.Options;
using PersonaScript.Modules.Identity.Application.Abstractions;
using PersonaScript.Modules.Identity.Domain;
using PersonaScript.Modules.Identity.Infrastructure.Security;

namespace PersonaScript.Modules.Identity.UnitTests.Infrastructure;

public class JwtTokenGeneratorTests
{
    [Fact]
    public void GenerateToken_ShouldIncludeTenantIdAndUserClaims()
    {
        var options = Options.Create(new JwtOptions
        {
            Issuer = "PersonaScriptTest",
            Audience = "PersonaScriptTestAudience",
            Secret = "Super_Secret_Test_Key_That_Is_Long_Enough_256_Bits!",
            ExpirationMinutes = 60
        });

        var generator = new JwtTokenGenerator(options);
        var user = User.Register("Ana Souza", "ana@example.com", "hash-123").Value;

        var tokenResult = generator.GenerateToken(user);

        tokenResult.Should().NotBeNull();
        tokenResult.TokenType.Should().Be("Bearer");
        tokenResult.UserId.Should().Be(user.Id);
        tokenResult.TenantId.Should().Be(user.TenantId);
        tokenResult.AccessToken.Should().NotBeNullOrWhiteSpace();

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(tokenResult.AccessToken);

        jwtToken.Issuer.Should().Be("PersonaScriptTest");
        jwtToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value.Should().Be(user.Id.ToString());
        jwtToken.Claims.First(c => c.Type == "tenant_id").Value.Should().Be(user.TenantId.ToString());
        jwtToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value.Should().Be("ana@example.com");
        jwtToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Name).Value.Should().Be("Ana Souza");
        jwtToken.Claims.First(c => c.Type == "role" || c.Type == System.Security.Claims.ClaimTypes.Role).Value.Should().Be("Subscriber");
    }
}

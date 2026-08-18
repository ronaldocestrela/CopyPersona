using System.Net;
using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace PersonaScript.Server.UnitTests.Auth;

public class AccountEndpointsIntegrationTests : IClassFixture<PersonaScriptWebApplicationFactory>
{
    private readonly PersonaScriptWebApplicationFactory _factory;

    public AccountEndpointsIntegrationTests(PersonaScriptWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Register_ShouldSetAuthCookieAndRedirectHome_WhenSuccessful()
    {
        var client = CreateClient();
        using var request = await CreateRegisterRequestAsync(client, acceptTerms: true);

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location!.OriginalString.Should().Be("/");
        response.Headers.TryGetValues("Set-Cookie", out var cookies).Should().BeTrue();
        cookies!.Should().Contain(cookie => cookie.StartsWith("PersonaScript.Auth=", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Register_ShouldRedirectToCadastroWithError_WhenTermsNotAccepted()
    {
        var client = CreateClient();
        using var request = await CreateRegisterRequestAsync(client, acceptTerms: false);

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location!.OriginalString.Should().StartWith("/cadastro?error=");
    }

    private HttpClient CreateClient() =>
        _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true,
        });

    private static async Task<HttpRequestMessage> CreateRegisterRequestAsync(HttpClient client, bool acceptTerms)
    {
        var (token, cookieHeader) = await GetAntiforgeryAsync(client, "/cadastro");

        var fields = new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token,
            ["FullName"] = "Maria Silva",
            ["Email"] = $"maria-{Guid.NewGuid():N}@example.com",
            ["Password"] = "password123",
        };

        if (acceptTerms)
        {
            fields["AcceptTerms"] = "true";
        }

        var request = new HttpRequestMessage(HttpMethod.Post, "/account/register")
        {
            Content = new FormUrlEncodedContent(fields),
        };

        if (!string.IsNullOrEmpty(cookieHeader))
        {
            request.Headers.Add("Cookie", cookieHeader);
        }

        return request;
    }

    private static async Task<(string Token, string CookieHeader)> GetAntiforgeryAsync(HttpClient client, string path)
    {
        using var response = await client.GetAsync(path);
        var html = await response.Content.ReadAsStringAsync();

        var match = Regex.Match(
            html,
            "name=\"__RequestVerificationToken\"\\s+value=\"([^\"]+)\"",
            RegexOptions.IgnoreCase);
        match.Success.Should().BeTrue("a página auth deve incluir AntiforgeryToken");

        if (!response.Headers.TryGetValues("Set-Cookie", out var setCookies))
        {
            return (match.Groups[1].Value, string.Empty);
        }

        var cookieHeader = string.Join("; ", setCookies.Select(static cookie =>
        {
            var semicolonIndex = cookie.IndexOf(';');
            return semicolonIndex >= 0 ? cookie[..semicolonIndex] : cookie;
        }));

        return (match.Groups[1].Value, cookieHeader);
    }
}

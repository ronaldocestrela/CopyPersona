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
        response.Headers.Location!.OriginalString.Should().BeOneOf("/", "/anamnese");
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

    [Fact]
    public async Task RequestPasswordReset_ShouldRedirectToEsqueciSenhaWithSuccess()
    {
        var client = CreateClient();
        var (token, cookieHeader) = await GetAntiforgeryAsync(client, "/esqueci-senha");

        var fields = new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token,
            ["Email"] = "maria-test@example.com",
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "/account/esqueci-senha")
        {
            Content = new FormUrlEncodedContent(fields),
        };
        if (!string.IsNullOrEmpty(cookieHeader))
        {
            request.Headers.Add("Cookie", cookieHeader);
        }

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location!.OriginalString.Should().Be("/esqueci-senha?success=true");
    }

    [Fact]
    public async Task ResetPassword_ShouldRedirectToRedefinirSenhaWithError_WhenPasswordsMismatch()
    {
        var client = CreateClient();
        var (token, cookieHeader) = await GetAntiforgeryAsync(client, "/redefinir-senha?email=maria@example.com&token=abc");

        var fields = new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token,
            ["Email"] = "maria@example.com",
            ["Token"] = "abc",
            ["Password"] = "password123",
            ["ConfirmPassword"] = "differentpassword",
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "/account/redefinir-senha")
        {
            Content = new FormUrlEncodedContent(fields),
        };
        if (!string.IsNullOrEmpty(cookieHeader))
        {
            request.Headers.Add("Cookie", cookieHeader);
        }

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location!.OriginalString.Should().Contain("error=");
    }

    [Fact]
    public async Task IssueToken_ShouldReturnJwtToken_WhenCredentialsAreValid()
    {
        var client = CreateClient();
        var email = $"jwt-user-{Guid.NewGuid():N}@example.com";
        var password = "password123";

        using (var registerRequest = await CreateCustomRegisterRequestAsync(client, "JWT User", email, password))
        {
            var regResponse = await client.SendAsync(registerRequest);
            regResponse.StatusCode.Should().Be(HttpStatusCode.Redirect);
        }

        var fields = new Dictionary<string, string>
        {
            ["Email"] = email,
            ["Password"] = password,
        };

        var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "/account/token")
        {
            Content = new FormUrlEncodedContent(fields),
        };

        var response = await client.SendAsync(tokenRequest);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("accessToken");
        json.Should().Contain("Bearer");
    }

    [Fact]
    public async Task IssueToken_ShouldReturnBadRequest_WhenCredentialsAreInvalid()
    {
        var client = CreateClient();
        var fields = new Dictionary<string, string>
        {
            ["Email"] = "nonexistent@example.com",
            ["Password"] = "wrongpassword",
        };

        var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "/account/token")
        {
            Content = new FormUrlEncodedContent(fields),
        };

        var response = await client.SendAsync(tokenRequest);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ExternalLogin_ShouldRedirectToError_WhenProviderIsInvalid()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/account/external-login/InvalidProvider");

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location!.OriginalString.Should().Contain("/login?error=");
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

    private static async Task<HttpRequestMessage> CreateCustomRegisterRequestAsync(HttpClient client, string fullName, string email, string password)
    {
        var (token, cookieHeader) = await GetAntiforgeryAsync(client, "/cadastro");

        var fields = new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token,
            ["FullName"] = fullName,
            ["Email"] = email,
            ["Password"] = password,
            ["AcceptTerms"] = "true",
        };

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

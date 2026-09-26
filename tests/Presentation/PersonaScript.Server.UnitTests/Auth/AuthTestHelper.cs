using System.Net.Http;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace PersonaScript.Server.UnitTests.Auth;

public static class AuthTestHelper
{
    public static async Task<HttpClient> CreateAuthenticatedClientAsync(PersonaScriptWebApplicationFactory factory)
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });

        var (token, cookieHeader) = await GetAntiforgeryAsync(client, "/cadastro");

        var fields = new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token,
            ["FullName"] = "Usuário Teste",
            ["Email"] = $"user.{Guid.NewGuid():N}@example.com",
            ["Password"] = "SenhaForte123!",
            ["AcceptTerms"] = "true"
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "/account/register")
        {
            Content = new FormUrlEncodedContent(fields)
        };

        if (!string.IsNullOrEmpty(cookieHeader))
        {
            request.Headers.Add("Cookie", cookieHeader);
        }

        var response = await client.SendAsync(request);
        if ((int)response.StatusCode < 200 || (int)response.StatusCode > 399)
        {
            throw new InvalidOperationException($"Register failed with status {response.StatusCode}");
        }

        return client;
    }

    private static async Task<(string Token, string CookieHeader)> GetAntiforgeryAsync(HttpClient client, string path)
    {
        using var response = await client.GetAsync(path);
        var html = await response.Content.ReadAsStringAsync();

        var match = Regex.Match(
            html,
            "name=\"__RequestVerificationToken\"\\s+value=\"([^\"]+)\"",
            RegexOptions.IgnoreCase);

        if (!match.Success)
        {
            throw new InvalidOperationException("Anti-forgery token not found");
        }

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

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Identity.Application.Abstractions;
using PersonaScript.Modules.Identity.Application.Commands.ExternalLogin;
using PersonaScript.Modules.Identity.Application.Commands.GenerateJwtToken;
using PersonaScript.Modules.Identity.Application.Commands.LoginUser;
using PersonaScript.Modules.Identity.Application.Commands.RegisterUser;
using PersonaScript.Modules.Identity.Application.Commands.RequestPasswordReset;
using PersonaScript.Modules.Identity.Application.Commands.ResetPassword;

namespace PersonaScript.Server.Endpoints;

public static class AccountEndpoints
{
    public static IEndpointRouteBuilder MapAccountEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/account/register", RegisterAsync);
        endpoints.MapPost("/account/login", LoginAsync);
        endpoints.MapPost("/account/esqueci-senha", RequestPasswordResetAsync);
        endpoints.MapPost("/account/redefinir-senha", ResetPasswordAsync);
        endpoints.MapGet("/account/external-login/{provider}", ExternalLoginAsync);
        endpoints.MapGet("/account/external-callback", ExternalCallbackAsync);
        endpoints.MapPost("/account/token", IssueJwtTokenAsync).DisableAntiforgery();
        return endpoints;
    }

    private static async Task<IResult> RegisterAsync(
        HttpContext context,
        ICommandHandler<RegisterUserCommand, LoginResult> handler,
        IAuthSession authSession,
        [FromForm] string fullName,
        [FromForm] string email,
        [FromForm] string password,
        [FromForm] bool acceptTerms = false)
    {
        var result = await handler.Handle(
            new RegisterUserCommand(fullName, email, password, acceptTerms),
            context.RequestAborted);

        if (result.IsFailure)
        {
            return Results.Redirect($"/cadastro?error={Uri.EscapeDataString(result.Error.Message)}");
        }

        await authSession.SignInAsync(
            new AuthUser(result.Value.UserId, result.Value.Email, result.Value.FullName, result.Value.Role),
            context.RequestAborted);

        return Results.Redirect("/");
    }

    private static async Task<IResult> LoginAsync(
        HttpContext context,
        ICommandHandler<LoginUserCommand, LoginResult> handler,
        IAuthSession authSession,
        [FromForm] string email,
        [FromForm] string password)
    {
        var result = await handler.Handle(
            new LoginUserCommand(email, password),
            context.RequestAborted);

        if (result.IsFailure)
        {
            return Results.Redirect($"/login?error={Uri.EscapeDataString(result.Error.Message)}");
        }

        await authSession.SignInAsync(
            new AuthUser(result.Value.UserId, result.Value.Email, result.Value.FullName, result.Value.Role),
            context.RequestAborted);

        return Results.Redirect("/");
    }

    private static async Task<IResult> RequestPasswordResetAsync(
        HttpContext context,
        ICommandHandler<RequestPasswordResetCommand> handler,
        [FromForm] string email)
    {
        var baseUrl = $"{context.Request.Scheme}://{context.Request.Host}";
        await handler.Handle(new RequestPasswordResetCommand(email, baseUrl), context.RequestAborted);
        return Results.Redirect("/esqueci-senha?success=true");
    }

    private static async Task<IResult> ResetPasswordAsync(
        HttpContext context,
        ICommandHandler<ResetPasswordCommand> handler,
        [FromForm] string email,
        [FromForm] string token,
        [FromForm] string password,
        [FromForm] string confirmPassword)
    {
        if (password != confirmPassword)
        {
            return Results.Redirect($"/redefinir-senha?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}&error={Uri.EscapeDataString("As senhas não coincidem.")}");
        }

        var result = await handler.Handle(
            new ResetPasswordCommand(email, token, password),
            context.RequestAborted);

        if (result.IsFailure)
        {
            return Results.Redirect($"/redefinir-senha?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}&error={Uri.EscapeDataString(result.Error.Message)}");
        }

        return Results.Redirect("/login?success=senha_redefinida");
    }

    private static IResult ExternalLoginAsync(
        HttpContext context,
        [FromRoute] string provider,
        [FromQuery] string? returnUrl = "/")
    {
        if (string.IsNullOrWhiteSpace(provider) ||
            (!string.Equals(provider, "Google", StringComparison.OrdinalIgnoreCase) &&
             !string.Equals(provider, "Apple", StringComparison.OrdinalIgnoreCase)))
        {
            return Results.Redirect($"/login?error={Uri.EscapeDataString("Provedor social inválido.")}");
        }

        var redirectUrl = $"/account/external-callback?provider={Uri.EscapeDataString(provider)}&returnUrl={Uri.EscapeDataString(returnUrl ?? "/")}";
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        return Results.Challenge(properties, [provider]);
    }

    private static async Task<IResult> ExternalCallbackAsync(
        HttpContext context,
        ICommandHandler<ExternalLoginCommand, LoginResult> handler,
        IAuthSession authSession,
        [FromQuery] string provider,
        [FromQuery] string? returnUrl = "/")
    {
        var authenticateResult = await context.AuthenticateAsync(provider);
        if (!authenticateResult.Succeeded || authenticateResult.Principal is null)
        {
            return Results.Redirect($"/login?error={Uri.EscapeDataString("Falha na autenticação social com o provedor.")}");
        }

        var email = authenticateResult.Principal.FindFirstValue(ClaimTypes.Email)
            ?? authenticateResult.Principal.FindFirstValue("email");

        var providerKey = authenticateResult.Principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? authenticateResult.Principal.FindFirstValue("sub");

        var fullName = authenticateResult.Principal.FindFirstValue(ClaimTypes.Name)
            ?? authenticateResult.Principal.FindFirstValue("name")
            ?? email;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(providerKey))
        {
            return Results.Redirect($"/login?error={Uri.EscapeDataString("E-mail não retornado pelo provedor social.")}");
        }

        var commandResult = await handler.Handle(
            new ExternalLoginCommand(provider, providerKey, email, fullName ?? email),
            context.RequestAborted);

        if (commandResult.IsFailure)
        {
            return Results.Redirect($"/login?error={Uri.EscapeDataString(commandResult.Error.Message)}");
        }

        await authSession.SignInAsync(
            new AuthUser(commandResult.Value.UserId, commandResult.Value.Email, commandResult.Value.FullName, commandResult.Value.Role),
            context.RequestAborted);

        return Results.Redirect(string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl);
    }

    public sealed record IssueTokenRequest(string? Email, string? Password);

    private static async Task<IResult> IssueJwtTokenAsync(
        HttpContext context,
        ICommandHandler<GenerateJwtTokenCommand, JwtTokenResult> handler)
    {
        string email = string.Empty;
        string password = string.Empty;

        if (context.Request.HasJsonContentType())
        {
            var request = await context.Request.ReadFromJsonAsync<IssueTokenRequest>(context.RequestAborted);
            email = request?.Email ?? string.Empty;
            password = request?.Password ?? string.Empty;
        }
        else if (context.Request.HasFormContentType)
        {
            var form = await context.Request.ReadFormAsync(context.RequestAborted);
            email = form["Email"].ToString();
            password = form["Password"].ToString();
        }

        var result = await handler.Handle(
            new GenerateJwtTokenCommand(email, password),
            context.RequestAborted);

        if (result.IsFailure)
        {
            return Results.Json(new { error = result.Error.Message, code = result.Error.Code }, statusCode: 400);
        }

        return Results.Ok(result.Value);
    }
}

using Microsoft.AspNetCore.Mvc;
using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Identity.Application.Abstractions;
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
            new AuthUser(result.Value.UserId, result.Value.Email, result.Value.FullName),
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
            new AuthUser(result.Value.UserId, result.Value.Email, result.Value.FullName),
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
}

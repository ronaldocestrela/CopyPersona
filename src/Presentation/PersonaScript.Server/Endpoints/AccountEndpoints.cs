using Microsoft.AspNetCore.Mvc;
using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.Modules.Identity.Application.Abstractions;
using PersonaScript.Modules.Identity.Application.Commands.LoginUser;
using PersonaScript.Modules.Identity.Application.Commands.RegisterUser;

namespace PersonaScript.Server.Endpoints;

public static class AccountEndpoints
{
    public static IEndpointRouteBuilder MapAccountEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/account/register", RegisterAsync);
        endpoints.MapPost("/account/login", LoginAsync);
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
}

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Billing.Infrastructure;
using PersonaScript.Modules.Identity.Infrastructure;
using PersonaScript.Modules.Personas.Infrastructure;
using PersonaScript.Modules.Scripts.Infrastructure;
using PersonaScript.Server.Components;
using PersonaScript.Server.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.Cookie.Name = "PersonaScript.Auth";
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddTenancy();
builder.Services.AddIdentityModule(builder.Configuration, builder.Environment);
builder.Services.AddBillingModule();
builder.Services.AddPersonasModule();
builder.Services.AddScriptsModule();

builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await app.Services.ApplyIdentityMigrationsAsync();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
app.MapAccountEndpoints();
app.MapGet("/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/login");
});

app.Run();

public partial class Program;

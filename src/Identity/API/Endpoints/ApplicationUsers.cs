using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using ServerGame.Api.Infrastructure;
using ServerGame.Application.ApplicationUsers.Commands;
using ServerGame.Domain.Rules;
using ServerGame.Domain.ValueObjects.Accounts;
using ServerGame.Infrastructure.Database.Application.Identity.Entities;

namespace ServerGame.Api.Endpoints;

public class ApplicationUsers : BaseIdentityEndpoints<ApplicationUser>
{
    public override void Map(WebApplication app)
    {
        var routeGroup = app.MapGroup(this);

        // --- Endpoints Públicos ---
        routeGroup.MapPost(Register, "/register");
        routeGroup.MapPost(Login, "/login");
        routeGroup.MapPost(Refresh, "/refresh");
        routeGroup.MapPost(ConfirmEmail, "/confirmEmail");
        routeGroup.MapPost(ResendConfirmationEmail, "/resendConfirmationEmail");
        routeGroup.MapPost(ForgotPassword, "/forgotPassword");
        routeGroup.MapPost(ResetPassword, "/resetPassword");

        // --- Endpoints de Gerenciamento (Protegidos) ---
        var accountGroup = routeGroup.MapGroup("/manage").RequireAuthorization();

        accountGroup.MapPost(ManageTwoFactorAuth, "/2fa");
        accountGroup.MapGet(GetInfo, "/info");
        accountGroup.MapPost(UpdateInfo, "/info");
    }

    protected override async Task<Results<Ok, ValidationProblem>> Register(RegisterCommand registration, HttpContext context, UserManager<ApplicationUser> userManager, IUserStore<ApplicationUser> userStore,
        IEmailSender<ApplicationUser> emailSender, LinkGenerator linkGenerator)
    {
        if (!userManager.SupportsUserEmail)
            throw new NotSupportedException($"{nameof(ApplicationUsers)} requer um user store com suporte a e-mail.");
        
        
        var userName = registration.Username.Trim().ToLowerInvariant();
        var email = registration.Email.Trim().ToLowerInvariant();

        if (!ApplicationUser.TryCreate(
                userName, 
                email, out var user, 
                out var errorMessage))
        {
            return CreateValidationProblem(IdentityResult.Failed(
                new IdentityError
                {
                    Code = nameof(errorMessage), Description = errorMessage ?? "Erro ao criar usuário."
                }));
        }
        
        var emailStore = (IUserEmailStore<ApplicationUser>)userStore;
        await userStore.SetUserNameAsync(user!, userName, CancellationToken.None);
        await emailStore.SetEmailAsync(user!, email, CancellationToken.None);
        
        var result = await userManager.CreateAsync(user!, registration.Password);

        if (!result.Succeeded)
            return CreateValidationProblem(result);

        await SendConfirmationEmailAsync(user!, userManager, context, linkGenerator, emailSender, email);
        return TypedResults.Ok();
        
    }
}

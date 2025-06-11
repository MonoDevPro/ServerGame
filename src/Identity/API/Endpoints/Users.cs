using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using ServerGame.Api.Infrastructure;
using ServerGame.Application.Users.Commands;
using ServerGame.Application.Users.Notifications;
using ServerGame.Infrastructure.Identity.Entities;

namespace ServerGame.Api.Endpoints;

public class Users : BaseIdentityEndpoints<ApplicationUser>
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
            throw new NotSupportedException($"{nameof(Users)} requer um user store com suporte a e-mail.");
        
        // TODO: Precisamos adicionar uma validação mais robust para o nome de usuário e e-mail, como verificar se já existem usuários com esses dados.
        var userName = registration.Username.Trim();
        
        if (string.IsNullOrEmpty(userName))
            return CreateValidationProblem(IdentityResult.Failed(userManager.ErrorDescriber.InvalidUserName(userName)));

        var emailStore = (IUserEmailStore<ApplicationUser>)userStore;
        var email = registration.Email;

        if (string.IsNullOrEmpty(email) || !_emailAddressAttribute.IsValid(email))
            return CreateValidationProblem(IdentityResult.Failed(userManager.ErrorDescriber.InvalidEmail(email)));

        var user = ApplicationUser.Create(userName, email);
        await userStore.SetUserNameAsync(user, userName, CancellationToken.None);
        await emailStore.SetEmailAsync(user, email, CancellationToken.None);
        
        var result = await userManager.CreateAsync(user, registration.Password);

        if (!result.Succeeded)
            return CreateValidationProblem(result);

        await SendConfirmationEmailAsync(user, userManager, context, linkGenerator, emailSender, email);
        return TypedResults.Ok();
        
    }
}

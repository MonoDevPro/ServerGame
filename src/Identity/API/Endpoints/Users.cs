using System.Security.Claims;
using Infra.Identity.Persistence.Entities;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ServerGame.Api.Models;
using ServerGame.Application.Accounts.Commands.Create;
using ServerGame.Application.Accounts.Queries.GetAccount;
using ServerGame.Application.Accounts.Services;
using ServerGame.Application.Common.Interfaces;
using ServerGame.Domain.Exceptions;
using ServerGame.Domain.Rules;

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
    
    protected override async Task<Results<Ok, ValidationProblem>> Register(
        [FromBody] RegisterRequest registration,
        HttpContext context,
        UserManager<ApplicationUser> userManager,
        IUserStore<ApplicationUser> userStore,
        IEmailSender<ApplicationUser> emailSender,
        LinkGenerator linkGenerator)
    {
        // Delegamos para o método base do Identity para lidar com o registro de usuário
        var register = await base.Register(registration, context, userManager, userStore, emailSender, linkGenerator);
        
        // A conta de domínio será criada no primeiro login do usuário
        return register;
    }

    protected override async Task<Results<Ok<AccessTokenResponse>, EmptyHttpResult, ProblemHttpResult>> Login(
        [FromBody] LoginRequest login,
        [FromQuery] bool? useCookies,
        [FromQuery] bool? useSessionCookies,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager)
    {
        // Chamar o método de login da classe base para autenticar o usuário
        var loginResult = await base.Login(login, useCookies, useSessionCookies, signInManager, userManager);
        
        // Se o login foi bem-sucedido (não retornou Empty nem Problem)
        if (loginResult.Result is ProblemHttpResult)
            return loginResult;

        // Se o login foi bem-sucedido, obtemos o usuário autenticado,
        // caso contrário a conta vaai ser criada no primeiro login e retornamos o resultado de login.
        var mediator = signInManager.Context.RequestServices.GetRequiredService<IMediator>();
        await mediator.Send(new GetAccountInfoQuery());

        return loginResult;
    }
}

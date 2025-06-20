using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using GameServer.Application.Accounts.Commands.Create;
using GameServer.Application.Users.Handlers;
using GameServer.Domain.Rules;
using GameServer.Web.Models;
using Identity.Persistence.Entities;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace GameServer.Web.Endpoints;

/// <summary>
/// Mapeia os endpoints da API de Identidade usando um padrão de grupo de endpoints baseado em classe.
/// </summary>
/// <typeparam name="ApplicationUser">O tipo que descreve o usuário.</typeparam>
public class Users : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var routeGroup = app.MapGroup(this);

        // --- Endpoints Públicos ---
        routeGroup.MapPost(Register, "/register");
        routeGroup.MapPost(Authenticate, "/login");
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

    // POST /register
    private async Task<Results<Ok, ValidationProblem>> Register(
        [FromBody] RegisterRequest registration,
        HttpContext context,
        UserManager<ApplicationUser> userManager,
        IUserStore<ApplicationUser> userStore,
        IEmailSender<ApplicationUser> emailSender,
        LinkGenerator linkGenerator,
        IPublisher publisher)
    {
        if (!userManager.SupportsUserEmail)
        {
            throw new NotSupportedException($"{nameof(ApplicationUser)} requires a user store with email support.");
        }
            
        var emailStore = (IUserEmailStore<ApplicationUser>)userStore;
        var email = registration.Email;

        if (string.IsNullOrEmpty(email) || !EmailRule.IsValid(email))
        {
            return CreateValidationProblem(IdentityResult.Failed(userManager.ErrorDescriber.InvalidEmail(email)));
        }

        var userName = registration.Username;
            
        if (string.IsNullOrEmpty(userName) || !UsernameRule.IsValid(userName))
        {
            return CreateValidationProblem(IdentityResult.Failed(userManager.ErrorDescriber.InvalidUserName(userName)));
        }

        var user = new ApplicationUser();
        await userStore.SetUserNameAsync(user, userName, CancellationToken.None);
        await emailStore.SetEmailAsync(user, email, CancellationToken.None);
        var result = await userManager.CreateAsync(user, registration.Password);

        if (!result.Succeeded)
        {
            return CreateValidationProblem(result);
        }
        
        await publisher.Publish(new UserCreatedNotification(user.Id), CancellationToken.None);

        await SendConfirmationEmailAsync(user, userManager, context, linkGenerator, emailSender, email, isChange: false);
        return TypedResults.Ok();
    }

    // POST /login
    private async Task<Results<Ok<AccessTokenResponse>, EmptyHttpResult, ProblemHttpResult>> Authenticate(
        [FromBody] LoginRequest login,
        [FromQuery] bool? useCookies,
        [FromQuery] bool? useSessionCookies,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IPublisher publisher)
    {
        var useCookieScheme = (useCookies == true) || (useSessionCookies == true);
            var isPersistent = (useCookies == true) && (useSessionCookies != true);
            signInManager.AuthenticationScheme = useCookieScheme ? IdentityConstants.ApplicationScheme : IdentityConstants.BearerScheme;

            
            if (!UsernameRule.IsValid(login.EmailOrUsername) &&
                !EmailRule.IsValid(login.EmailOrUsername))
            {
                return TypedResults.Empty;
            }
            
            var username = UsernameRule.IsValid(login.EmailOrUsername) 
                ? login.EmailOrUsername.Trim().ToLowerInvariant()
                : null;
            var email = EmailRule.IsValid(login.EmailOrUsername) 
                ? login.EmailOrUsername.Trim().ToLowerInvariant()
                : null;
            
            if (email is not null)
            {
                var user = await userManager.FindByEmailAsync(email);
                if (user is null)
                    return TypedResults.Problem("Invalid email or username.", statusCode: StatusCodes.Status401Unauthorized);
                
                username = await userManager.GetUserNameAsync(user);
            }
            else if (username is not null)
            {
                var user = await userManager.FindByNameAsync(username);
                if (user is null)
                    return TypedResults.Problem("Invalid email or username.", statusCode: StatusCodes.Status401Unauthorized);
                
                username = await userManager.GetUserNameAsync(user);
            }
            else
            {
                return TypedResults.Problem("Invalid email or username.", statusCode: StatusCodes.Status401Unauthorized);
            }
            
            Guard.Against.NullOrEmpty(username, nameof(login.EmailOrUsername));
            
            var result = await signInManager.PasswordSignInAsync(username, login.Password, isPersistent, lockoutOnFailure: true);

            if (result.RequiresTwoFactor)
            {
                if (!string.IsNullOrEmpty(login.TwoFactorCode))
                {
                    result = await signInManager.TwoFactorAuthenticatorSignInAsync(login.TwoFactorCode, isPersistent, rememberClient: isPersistent);
                }
                else if (!string.IsNullOrEmpty(login.TwoFactorRecoveryCode))
                {
                    result = await signInManager.TwoFactorRecoveryCodeSignInAsync(login.TwoFactorRecoveryCode);
                }
            }

            if (!result.Succeeded)
                return TypedResults.Problem(result.ToString(), statusCode: StatusCodes.Status401Unauthorized);
            
            // Aqui você dispara o comando — o FluentValidation já vai rodar o CreateAccountCommandValidator
            await publisher.Publish(new UserAuthenticatedNotification(), CancellationToken.None);

            // The signInManager already produced the needed response in the form of a cookie or bearer token.
            return TypedResults.Empty;
    }
    
    // POST /refresh
    private async Task<Results<Ok<AccessTokenResponse>, UnauthorizedHttpResult, SignInHttpResult, ChallengeHttpResult>> Refresh(
        [FromBody] RefreshRequest refreshRequest,
        SignInManager<ApplicationUser> signInManager,
        IOptionsMonitor<BearerTokenOptions> bearerTokenOptions,
        TimeProvider timeProvider)
    {
        var refreshTokenProtector = bearerTokenOptions.Get(IdentityConstants.BearerScheme).RefreshTokenProtector;
        var refreshTicket = refreshTokenProtector.Unprotect(refreshRequest.RefreshToken);

        if (refreshTicket?.Properties?.ExpiresUtc is not { } expiresUtc ||
            timeProvider.GetUtcNow() >= expiresUtc ||
            await signInManager.ValidateSecurityStampAsync(refreshTicket.Principal) is not { } user)
        {
            return TypedResults.Challenge();
        }

        var newPrincipal = await signInManager.CreateUserPrincipalAsync(user);
        return TypedResults.SignIn(newPrincipal, authenticationScheme: IdentityConstants.BearerScheme);
    }

    // GET /confirmEmail
    private async Task<Results<ContentHttpResult, UnauthorizedHttpResult>> ConfirmEmail(
        [FromQuery] string userId,
        [FromQuery] string code,
        [FromQuery] string? changedEmail,
        UserManager<ApplicationUser> userManager)
    {
        if (await userManager.FindByIdAsync(userId) is not { } user)
        {
            return TypedResults.Unauthorized();
        }

        try
        {
            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        }
        catch (FormatException)
        {
            return TypedResults.Unauthorized();
        }

        IdentityResult result;
        if (string.IsNullOrEmpty(changedEmail))
        {
            result = await userManager.ConfirmEmailAsync(user, code);
        }
        else
        {
            result = await userManager.ChangeEmailAsync(user, changedEmail, code);
            if (result.Succeeded)
            {
                //result = await userManager.SeApplicationUserNameAsync(user, changedEmail);
            }
        }

        if (!result.Succeeded)
        {
            return TypedResults.Unauthorized();
        }

        return TypedResults.Text("Thank you for confirming your email.");
    }
    
    // POST /resendConfirmationEmail
    private async Task<Ok> ResendConfirmationEmail(
        [FromBody] ResendConfirmationEmailRequest resendRequest,
        HttpContext context,
        UserManager<ApplicationUser> userManager,
        IEmailSender<ApplicationUser> emailSender,
        LinkGenerator linkGenerator)
    {
        if (await userManager.FindByEmailAsync(resendRequest.Email) is not { } user)
        {
            return TypedResults.Ok();
        }

        await SendConfirmationEmailAsync(user, userManager, context, linkGenerator, emailSender, resendRequest.Email);
        return TypedResults.Ok();
    }
    
    // POST /forgotPassword
    private async Task<Results<Ok, ValidationProblem>> ForgotPassword(
        [FromBody] ForgotPasswordRequest resetRequest,
        UserManager<ApplicationUser> userManager,
        IEmailSender<ApplicationUser> emailSender)
    {
        var user = await userManager.FindByEmailAsync(resetRequest.Email);

        if (user is not null && await userManager.IsEmailConfirmedAsync(user))
        {
            var code = await userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            await emailSender.SendPasswordResetCodeAsync(user, resetRequest.Email, HtmlEncoder.Default.Encode(code));
        }

        return TypedResults.Ok();
    }

    // POST /resetPassword
    private async Task<Results<Ok, ValidationProblem>> ResetPassword(
        [FromBody] ResetPasswordRequest resetRequest,
        UserManager<ApplicationUser> userManager)
    {
        var user = await userManager.FindByEmailAsync(resetRequest.Email);

        if (user is null || !(await userManager.IsEmailConfirmedAsync(user)))
        {
            return CreateValidationProblem(IdentityResult.Failed(userManager.ErrorDescriber.InvalidToken()));
        }

        IdentityResult result;
        try
        {
            var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(resetRequest.ResetCode));
            result = await userManager.ResetPasswordAsync(user, code, resetRequest.NewPassword);
        }
        catch (FormatException)
        {
            result = IdentityResult.Failed(userManager.ErrorDescriber.InvalidToken());
        }

        if (!result.Succeeded)
        {
            return CreateValidationProblem(result);
        }

        return TypedResults.Ok();
    }
    
    // GET /manage/info
    private async Task<Results<Ok<InfoResponse>, ValidationProblem, NotFound>> GetInfo(
        ClaimsPrincipal claimsPrincipal,
        UserManager<ApplicationUser> userManager)
    {
        if (await userManager.GetUserAsync(claimsPrincipal) is not { } user)
        {
            return TypedResults.NotFound();
        }
        return TypedResults.Ok(await CreateInfoResponseAsync(user, userManager));
    }
    
    // POST /manage/info
    private async Task<Results<Ok<InfoResponse>, ValidationProblem, NotFound>> UpdateInfo(
        ClaimsPrincipal claimsPrincipal,
        [FromBody] InfoRequest infoRequest,
        HttpContext context,
        UserManager<ApplicationUser> userManager,
        IEmailSender<ApplicationUser> emailSender,
        LinkGenerator linkGenerator)
    {
        if (await userManager.GetUserAsync(claimsPrincipal) is not { } user)
        {
            return TypedResults.NotFound();
        }

        if (!string.IsNullOrEmpty(infoRequest.NewEmail) && !EmailRule.IsValid(infoRequest.NewEmail))
        {
            return CreateValidationProblem(IdentityResult.Failed(userManager.ErrorDescriber.InvalidEmail(infoRequest.NewEmail)));
        }
        
        if (!string.IsNullOrEmpty(infoRequest.NewPassword))
        {
            if (string.IsNullOrEmpty(infoRequest.OldPassword))
            {
                return CreateValidationProblem("OldPasswordRequired", "The old password is required to set a new password.");
            }
            var changePasswordResult = await userManager.ChangePasswordAsync(user, infoRequest.OldPassword, infoRequest.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                return CreateValidationProblem(changePasswordResult);
            }
        }

        if (!string.IsNullOrEmpty(infoRequest.NewEmail))
        {
            var email = await userManager.GetEmailAsync(user);
            if (email != infoRequest.NewEmail)
            {
                await SendConfirmationEmailAsync(user, userManager, context, linkGenerator, emailSender, infoRequest.NewEmail, isChange: true);
            }
        }
        
        return TypedResults.Ok(await CreateInfoResponseAsync(user, userManager));
    }

    // POST /manage/2fa
    private async Task<Results<Ok<TwoFactorResponse>, ValidationProblem, NotFound>> ManageTwoFactorAuth(
        ClaimsPrincipal claimsPrincipal,
        [FromBody] TwoFactorRequest tfaRequest,
        SignInManager<ApplicationUser> signInManager)
    {
        var userManager = signInManager.UserManager;
        if (await userManager.GetUserAsync(claimsPrincipal) is not { } user)
        {
            return TypedResults.NotFound();
        }

        if (tfaRequest.Enable == true)
        {
            // Lógica para habilitar 2FA
            if (tfaRequest.ResetSharedKey)
            {
                return CreateValidationProblem("CannotResetSharedKeyAndEnable", "Resetting the 2fa shared key must disable 2fa...");
            }
            if (string.IsNullOrEmpty(tfaRequest.TwoFactorCode))
            {
                return CreateValidationProblem("RequiresTwoFactor", "No 2fa token was provided...");
            }
            if (!await userManager.VerifyTwoFactorTokenAsync(user, userManager.Options.Tokens.AuthenticatorTokenProvider, tfaRequest.TwoFactorCode))
            {
                return CreateValidationProblem("InvalidTwoFactorCode", "The 2fa token provided was invalid.");
            }
            await userManager.SetTwoFactorEnabledAsync(user, true);
        }
        else if (tfaRequest.Enable == false || tfaRequest.ResetSharedKey)
        {
            await userManager.SetTwoFactorEnabledAsync(user, false);
        }

        if (tfaRequest.ResetSharedKey)
        {
            await userManager.ResetAuthenticatorKeyAsync(user);
        }
        
        string[]? recoveryCodes = null;
        if (tfaRequest.ResetRecoveryCodes || (tfaRequest.Enable == true && await userManager.CountRecoveryCodesAsync(user) == 0))
        {
            var recoveryCodesEnumerable = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
            recoveryCodes = recoveryCodesEnumerable?.ToArray();
        }
        
        if (tfaRequest.ForgetMachine)
        {
            await signInManager.ForgetTwoFactorClientAsync();
        }

        var key = await userManager.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrEmpty(key))
        {
            await userManager.ResetAuthenticatorKeyAsync(user);
            key = await userManager.GetAuthenticatorKeyAsync(user) ?? throw new NotSupportedException("The user manager must produce an authenticator key after reset.");
        }

        return TypedResults.Ok(new TwoFactorResponse
        (
            key,
            recoveryCodes?.Length ?? await userManager.CountRecoveryCodesAsync(user),
            recoveryCodes,
            await userManager.GetTwoFactorEnabledAsync(user),
            await signInManager.IsTwoFactorClientRememberedAsync(user)
        ));
    }

    #region Helper Methods

    protected async Task SendConfirmationEmailAsync(ApplicationUser user, UserManager<ApplicationUser> userManager, HttpContext context, LinkGenerator linkGenerator, IEmailSender<ApplicationUser> emailSender, string email, bool isChange = false)
    {
        var code = isChange
            ? await userManager.GenerateChangeEmailTokenAsync(user, email)
            : await userManager.GenerateEmailConfirmationTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        var userId = await userManager.GetUserIdAsync(user);

        var routeValues = new RouteValueDictionary()
        {
            ["userId"] = userId,
            ["code"] = code,
        };

        if (isChange)
        {
            routeValues.Add("changedEmail", email);
        }
        
        // Usamos o nome do endpoint definido em Map() para gerar a URL
        var confirmEmailUrl = linkGenerator.GetUriByName(context, "ConfirmEmail", routeValues)
                              ?? throw new NotSupportedException("Could not find endpoint named 'ConfirmEmail'.");

        await emailSender.SendConfirmationLinkAsync(user, email, HtmlEncoder.Default.Encode(confirmEmailUrl));
    }

    private static ValidationProblem CreateValidationProblem(string errorCode, string errorDescription) =>
        TypedResults.ValidationProblem(new Dictionary<string, string[]> {
            { errorCode, [errorDescription] }
        });

    protected static ValidationProblem CreateValidationProblem(IdentityResult result)
    {
        Debug.Assert(!result.Succeeded);
        var errorDictionary = new Dictionary<string, string[]>(1);

        foreach (var error in result.Errors)
        {
            if (errorDictionary.TryGetValue(error.Code, out var descriptions))
            {
                var newDescriptions = new string[descriptions.Length + 1];
                Array.Copy(descriptions, newDescriptions, descriptions.Length);
                newDescriptions[descriptions.Length] = error.Description;
                errorDictionary[error.Code] = newDescriptions;
            }
            else
            {
                errorDictionary[error.Code] = [error.Description];
            }
        }
        return TypedResults.ValidationProblem(errorDictionary);
    }
    
    private static async Task<InfoResponse> CreateInfoResponseAsync(ApplicationUser user, UserManager<ApplicationUser> userManager)
    {
        var username = await userManager.GetUserNameAsync(user) ??
                       throw new NotSupportedException("Users must have a username.");
        var email = await userManager.GetEmailAsync(user) ??
                    throw new NotSupportedException("Users must have a username.");
        var isEmailConfirmed = await userManager.IsEmailConfirmedAsync(user);

        return new InfoResponse(username, email, isEmailConfirmed);
    }

    #endregion
}

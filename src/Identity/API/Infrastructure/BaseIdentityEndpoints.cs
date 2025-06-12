using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using ServerGame.Application.ApplicationUsers.Commands;
using ServerGame.Application.ApplicationUsers.Models;

namespace ServerGame.Api.Infrastructure;

/// <summary>
/// Mapeia os endpoints da API de Identidade usando um padrão de grupo de endpoints baseado em classe.
/// </summary>
/// <typeparam name="TUser">O tipo que descreve o usuário.</typeparam>
public abstract class BaseIdentityEndpoints<TUser> : EndpointGroupBase 
    where TUser : class, new()
{
    // Validador de e-mail reutilizado, conforme o código original.
    protected static readonly EmailAddressAttribute _emailAddressAttribute = new();

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

    // POST /register
    protected virtual async Task<Results<Ok, ValidationProblem>> Register(
        [FromBody] RegisterCommand registration,
        HttpContext context,
        UserManager<TUser> userManager,
        IUserStore<TUser> userStore,
        IEmailSender<TUser> emailSender,
        LinkGenerator linkGenerator)
    {
        if (!userManager.SupportsUserEmail)
        {
            throw new NotSupportedException($"{nameof(BaseIdentityEndpoints<TUser>)} requer um user store com suporte a e-mail.");
        }

        var emailStore = (IUserEmailStore<TUser>)userStore;
        var email = registration.Email;

        if (string.IsNullOrEmpty(email) || !_emailAddressAttribute.IsValid(email))
        {
            return CreateValidationProblem(IdentityResult.Failed(userManager.ErrorDescriber.InvalidEmail(email)));
        }

        var user = new TUser();
        await userStore.SetUserNameAsync(user, registration.Username, CancellationToken.None);
        await emailStore.SetEmailAsync(user, email, CancellationToken.None);
        var result = await userManager.CreateAsync(user, registration.Password);

        if (!result.Succeeded)
        {
            return CreateValidationProblem(result);
        }

        await SendConfirmationEmailAsync(user, userManager, context, linkGenerator, emailSender, email);
        return TypedResults.Ok();
    }

    // POST /login
    protected virtual async Task<Results<Ok<AccessTokenResponse>, EmptyHttpResult, ProblemHttpResult>> Login(
        [FromBody] LoginCommand login,
        [FromQuery] bool? useCookies,
        [FromQuery] bool? useSessionCookies,
        SignInManager<TUser> signInManager)
    {
        var useCookieScheme = (useCookies == true) || (useSessionCookies == true);
        var isPersistent = (useCookies == true) && (useSessionCookies != true);
        signInManager.AuthenticationScheme = useCookieScheme ? IdentityConstants.ApplicationScheme : IdentityConstants.BearerScheme;

        var result = await signInManager.PasswordSignInAsync(login.Username, login.Password, isPersistent, lockoutOnFailure: true);

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
        {
            return TypedResults.Problem(result.ToString(), statusCode: StatusCodes.Status401Unauthorized);
        }

        return TypedResults.Empty;
    }
    
    // POST /refresh
    protected virtual async Task<Results<Ok<AccessTokenResponse>, UnauthorizedHttpResult, SignInHttpResult, ChallengeHttpResult>> Refresh(
        [FromBody] RefreshCommand refreshCommand,
        SignInManager<TUser> signInManager,
        IOptionsMonitor<BearerTokenOptions> bearerTokenOptions,
        TimeProvider timeProvider)
    {
        var refreshTokenProtector = bearerTokenOptions.Get(IdentityConstants.BearerScheme).RefreshTokenProtector;
        var refreshTicket = refreshTokenProtector.Unprotect(refreshCommand.RefreshToken);

        if (refreshTicket?.Properties?.ExpiresUtc is not { } expiresUtc ||
            timeProvider.GetUtcNow() >= expiresUtc ||
            await signInManager.ValidateSecurityStampAsync(refreshTicket.Principal) is not TUser user)
        {
            return TypedResults.Challenge();
        }

        var newPrincipal = await signInManager.CreateUserPrincipalAsync(user);
        return TypedResults.SignIn(newPrincipal, authenticationScheme: IdentityConstants.BearerScheme);
    }

    // GET /confirmEmail
    protected virtual async Task<Results<ContentHttpResult, UnauthorizedHttpResult>> ConfirmEmail(
        [FromQuery] string userId,
        [FromQuery] string code,
        [FromQuery] string? changedEmail,
        UserManager<TUser> userManager)
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
                //result = await userManager.SetUserNameAsync(user, changedEmail);
            }
        }

        if (!result.Succeeded)
        {
            return TypedResults.Unauthorized();
        }

        return TypedResults.Text("Thank you for confirming your email.");
    }
    
    // POST /resendConfirmationEmail
    protected virtual async Task<Ok> ResendConfirmationEmail(
        [FromBody] ResendConfirmationEmailCommand resendCommand,
        HttpContext context,
        UserManager<TUser> userManager,
        IEmailSender<TUser> emailSender,
        LinkGenerator linkGenerator)
    {
        if (await userManager.FindByEmailAsync(resendCommand.Email) is not { } user)
        {
            return TypedResults.Ok();
        }

        await SendConfirmationEmailAsync(user, userManager, context, linkGenerator, emailSender, resendCommand.Email);
        return TypedResults.Ok();
    }
    
    // POST /forgotPassword
    protected virtual async Task<Results<Ok, ValidationProblem>> ForgotPassword(
        [FromBody] ForgotPasswordCommand resetCommand,
        UserManager<TUser> userManager,
        IEmailSender<TUser> emailSender)
    {
        var user = await userManager.FindByEmailAsync(resetCommand.Email);

        if (user is not null && await userManager.IsEmailConfirmedAsync(user))
        {
            var code = await userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            await emailSender.SendPasswordResetCodeAsync(user, resetCommand.Email, HtmlEncoder.Default.Encode(code));
        }

        return TypedResults.Ok();
    }

    // POST /resetPassword
    protected virtual async Task<Results<Ok, ValidationProblem>> ResetPassword(
        [FromBody] ResetPasswordCommand resetCommand,
        UserManager<TUser> userManager)
    {
        var user = await userManager.FindByEmailAsync(resetCommand.Email);

        if (user is null || !(await userManager.IsEmailConfirmedAsync(user)))
        {
            return CreateValidationProblem(IdentityResult.Failed(userManager.ErrorDescriber.InvalidToken()));
        }

        IdentityResult result;
        try
        {
            var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(resetCommand.ResetCode));
            result = await userManager.ResetPasswordAsync(user, code, resetCommand.NewPassword);
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
    protected virtual async Task<Results<Ok<InfoResponse>, ValidationProblem, NotFound>> GetInfo(
        ClaimsPrincipal claimsPrincipal,
        UserManager<TUser> userManager)
    {
        if (await userManager.GetUserAsync(claimsPrincipal) is not { } user)
        {
            return TypedResults.NotFound();
        }
        return TypedResults.Ok(await CreateInfoResponseAsync(user, userManager));
    }
    
    // POST /manage/info
    protected virtual async Task<Results<Ok<InfoResponse>, ValidationProblem, NotFound>> UpdateInfo(
        ClaimsPrincipal claimsPrincipal,
        [FromBody] InfoCommand infoCommand,
        HttpContext context,
        UserManager<TUser> userManager,
        IEmailSender<TUser> emailSender,
        LinkGenerator linkGenerator)
    {
        if (await userManager.GetUserAsync(claimsPrincipal) is not { } user)
        {
            return TypedResults.NotFound();
        }

        if (!string.IsNullOrEmpty(infoCommand.NewEmail) && !_emailAddressAttribute.IsValid(infoCommand.NewEmail))
        {
            return CreateValidationProblem(IdentityResult.Failed(userManager.ErrorDescriber.InvalidEmail(infoCommand.NewEmail)));
        }
        
        if (!string.IsNullOrEmpty(infoCommand.NewPassword))
        {
            if (string.IsNullOrEmpty(infoCommand.OldPassword))
            {
                return CreateValidationProblem("OldPasswordRequired", "The old password is required to set a new password.");
            }
            var changePasswordResult = await userManager.ChangePasswordAsync(user, infoCommand.OldPassword, infoCommand.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                return CreateValidationProblem(changePasswordResult);
            }
        }

        if (!string.IsNullOrEmpty(infoCommand.NewEmail))
        {
            var email = await userManager.GetEmailAsync(user);
            if (email != infoCommand.NewEmail)
            {
                await SendConfirmationEmailAsync(user, userManager, context, linkGenerator, emailSender, infoCommand.NewEmail, isChange: true);
            }
        }
        
        return TypedResults.Ok(await CreateInfoResponseAsync(user, userManager));
    }

    // POST /manage/2fa
    protected virtual async Task<Results<Ok<TwoFactorResponse>, ValidationProblem, NotFound>> ManageTwoFactorAuth(
        ClaimsPrincipal claimsPrincipal,
        [FromBody] TwoFactorCommand tfaCommand,
        SignInManager<TUser> signInManager)
    {
        var userManager = signInManager.UserManager;
        if (await userManager.GetUserAsync(claimsPrincipal) is not { } user)
        {
            return TypedResults.NotFound();
        }

        if (tfaCommand.Enable == true)
        {
            // Lógica para habilitar 2FA
            if (tfaCommand.ResetSharedKey)
            {
                return CreateValidationProblem("CannotResetSharedKeyAndEnable", "Resetting the 2fa shared key must disable 2fa...");
            }
            if (string.IsNullOrEmpty(tfaCommand.TwoFactorCode))
            {
                return CreateValidationProblem("RequiresTwoFactor", "No 2fa token was provided...");
            }
            if (!await userManager.VerifyTwoFactorTokenAsync(user, userManager.Options.Tokens.AuthenticatorTokenProvider, tfaCommand.TwoFactorCode))
            {
                return CreateValidationProblem("InvalidTwoFactorCode", "The 2fa token provided was invalid.");
            }
            await userManager.SetTwoFactorEnabledAsync(user, true);
        }
        else if (tfaCommand.Enable == false || tfaCommand.ResetSharedKey)
        {
            await userManager.SetTwoFactorEnabledAsync(user, false);
        }

        if (tfaCommand.ResetSharedKey)
        {
            await userManager.ResetAuthenticatorKeyAsync(user);
        }
        
        string[]? recoveryCodes = null;
        if (tfaCommand.ResetRecoveryCodes || (tfaCommand.Enable == true && await userManager.CountRecoveryCodesAsync(user) == 0))
        {
            var recoveryCodesEnumerable = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
            recoveryCodes = recoveryCodesEnumerable?.ToArray();
        }
        
        if (tfaCommand.ForgetMachine)
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

    protected async Task SendConfirmationEmailAsync(TUser user, UserManager<TUser> userManager, HttpContext context, LinkGenerator linkGenerator, IEmailSender<TUser> emailSender, string email, bool isChange = false)
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
    
    private static async Task<InfoResponse> CreateInfoResponseAsync<TUserResponse>(TUserResponse user, UserManager<TUserResponse> userManager) where TUserResponse : class
    {
        return new InfoResponse
        (
            await userManager.GetEmailAsync(user) ?? throw new NotSupportedException("Users must have an email."),
            await userManager.IsEmailConfirmedAsync(user)
        );
    }

    #endregion
}

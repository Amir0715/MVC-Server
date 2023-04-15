using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MVCS.Infrastructure.Identity;
using MVCS.Infrastructure.Identity.Services;

namespace MVCS.Presentation.gRPC.AuthenticationSchemeHandlers;

public class UserKeyAuthenticationSchemeOption : AuthenticationSchemeOptions
{

}

public class UserKeyAuthenticationSchemeHandler : AuthenticationHandler<UserKeyAuthenticationSchemeOption>
{
    private readonly IdentityDbContext _identityDbContext;
    private readonly KeyHasher _keyHasher;
    private readonly PasswordHasher<ApplicationUser> _passwordHasher;

    public UserKeyAuthenticationSchemeHandler(
        IOptionsMonitor<UserKeyAuthenticationSchemeOption> options, 
        ILoggerFactory logger, 
        UrlEncoder encoder, 
        ISystemClock clock,
        IdentityDbContext identityDbContext,
        KeyHasher keyHasher) : base(options, logger, encoder, clock)
    {
        _identityDbContext = identityDbContext ?? throw new ArgumentNullException(nameof(identityDbContext));
        _keyHasher = keyHasher ?? throw new ArgumentNullException(nameof(keyHasher));
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var userKey = this.Request.Headers.Authorization;
        
        if (userKey.Count == 0)
        {
            Logger.LogInformation("В заголовке Authorization запроса {traceId} ключ отсутствует", Context.TraceIdentifier);
            return AuthenticateResult.Fail("Необходимо передать ключ в заголовке Authorization");
        }

        this.Logger.LogInformation("Попытка аутентифицирован запрос {traceId} с помощью ключа", Context.TraceIdentifier);
        var hashedUserKey = _keyHasher.HashKey(userKey);
        
        var user = await _identityDbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Key == hashedUserKey);

        if (user != null)
        {
            Logger.LogInformation("Пользователь {userId} аутентифицирован с помощью ключа", user.Id);
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, this.Scheme.Name);
            return AuthenticateResult.Success(ticket);
        }

        return AuthenticateResult.Fail("Пользователь не найден");
    }
}
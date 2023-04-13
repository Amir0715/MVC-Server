using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using MVCS.Core.Domain.Interfaces;
using MVCS.Infrastructure.Identity;

namespace MVCS.Presentation.gRPC.Services;

public class AuthService : Auth.AuthBase
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenClaimsService _tokenClaimsService;
    private readonly ILogger<AuthService> _logger;
    private readonly KeyHasher _keyHasher;

    public AuthService(
        SignInManager<ApplicationUser> signInManager, 
        UserManager<ApplicationUser> userManager, 
        ITokenClaimsService tokenClaimsService, 
        ILogger<AuthService> logger, 
        KeyHasher keyHasher)
    {
        _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _tokenClaimsService = tokenClaimsService ?? throw new ArgumentNullException(nameof(tokenClaimsService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _keyHasher = keyHasher ?? throw new ArgumentNullException(nameof(keyHasher));
    }

    public override async Task<TokenResponse> Login(LoginRequest request, ServerCallContext context)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            // TODO: Нужно правильно отдавать ошибку
            _logger.LogError("Пользователь с почтой {email} не найден", request.Email);
            return null;
        }

        var result = await _signInManager.CheckPasswordSignInAsync(
            user,
            request.Password,
            true
        );


        if (!result.Succeeded) return null;

        var token = await _tokenClaimsService.GetTokenAsync(request.Email);
        return new TokenResponse()
        {
            AuthToken = token
        };
    }

    public override async Task<TokenResponse> Registration(RegistrationRequest request, ServerCallContext context)
    {
        var applicationUser = new ApplicationUser
        {
            UserName = request.Username,
            Email = request.Email
        };

        var creationIdentityResult = await _userManager.CreateAsync(applicationUser, request.Password);
        if (!creationIdentityResult.Succeeded)
        {
            // TODO: Нужно правильно отдавать ошибку
            foreach (var identityError in creationIdentityResult.Errors)
            {
                _logger.LogError(identityError.Description);
            }

            return null;
        }

        var token = await _tokenClaimsService.GetTokenAsync(request.Email);
        return new TokenResponse()
        {
            AuthToken = token
        };
    }

    public override async Task<Empty> RegistrationWithKey(RegistrationWithKeyRequest request, ServerCallContext context)
    {
        var applicationUser = new ApplicationUser
        {
            UserName = request.Username,
            Key = _keyHasher.HashKey(request.Key),
        };
        var creationIdentityResult = await _userManager.CreateAsync(applicationUser);
        if (!creationIdentityResult.Succeeded)
        {
            // TODO: Нужно правильно отдавать ошибку
            foreach (var identityError in creationIdentityResult.Errors)
            {
                _logger.LogError(identityError.Description);
            }

            return null;
        }

        return new Empty();
    }

    [Authorize]
    public override async Task<MeMessage> Me(Empty request, ServerCallContext context)
    {
        var userPrincipal = context.GetHttpContext().User;
        var user = await _userManager.GetUserAsync(userPrincipal);
        
        return new MeMessage
        {
            Username = user.UserName
        };
    }
}
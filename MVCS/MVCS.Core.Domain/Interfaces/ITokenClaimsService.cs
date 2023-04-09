namespace MVCS.Core.Domain.Interfaces;

public interface ITokenClaimsService
{
    Task<string> GetTokenAsync(string email);
}
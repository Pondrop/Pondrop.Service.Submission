using Pondrop.Service.Submission.Api.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Pondrop.Service.Submission.Api.Services.Interfaces;
public interface ITokenProvider
{
    string AuthenticateShopper(TokenRequest request);

    ClaimsPrincipal ValidateToken(string token);

    string GetClaim(ClaimsPrincipal principal, string claimName);
}


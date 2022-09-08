using Pondrop.Service.Submission.Application.Interfaces.Services;
using Pondrop.Service.Submission.Domain.Models;

namespace Pondrop.Service.Submission.Api.Services;

public class UserService : IUserService
{
    public string GetMaterializedViewUserName() => "materialized_view";

    public string UserId { get; private set; }
    public string UserName { get; private set; }

    public UserService()
    {
        UserId = "admin";
        UserName = "admin";
    }

    public string CurrentUserId() => UserId;
    public string CurrentUserName() => UserName;

    public bool SetCurrentUser(UserModel user)
    {
        if (user is null)
            return false;

        UserId = user.Id ?? string.Empty;
        UserName = user.Email ?? string.Empty;
        return true;
    }
}
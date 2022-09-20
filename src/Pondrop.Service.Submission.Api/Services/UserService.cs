using Pondrop.Service.Submission.Application.Interfaces.Services;
using Pondrop.Service.Submission.Domain.Enums.User;
using Pondrop.Service.Submission.Domain.Models;

namespace Pondrop.Service.Submission.Api.Services;

public class UserService : IUserService
{
    public string GetMaterializedViewUserName() => "materialized_view";

    public string UserId { get; private set; }
    public string UserName { get; private set; }

    public UserType UserType { get; private set; }

    public UserService()
    {
        UserId = "admin";
        UserName = "admin";
        UserType = Domain.Enums.User.UserType.Shopper;
    }

    public string CurrentUserId() => UserId;
    public string CurrentUserName() => UserName;
    public UserType CurrentUserType() => UserType;

    public bool SetCurrentUser(UserModel user)
    {
        if (user is null)
            return false;

        UserId = user.Id ?? string.Empty;
        UserName = user.Email ?? string.Empty;
        UserType = Enum.TryParse(user.Type, out UserType userType) ? userType : UserType.Shopper;
        return true;
    }
}
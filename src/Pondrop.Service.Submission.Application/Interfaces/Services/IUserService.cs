using Pondrop.Service.Submission.Domain.Enums.User;
using Pondrop.Service.Submission.Domain.Models;

namespace Pondrop.Service.Submission.Application.Interfaces.Services;

public interface IUserService
{
    string GetMaterializedViewUserName();

    string CurrentUserId();

    string CurrentUserName();

    UserType CurrentUserType();

    bool SetCurrentUser(UserModel user);
}
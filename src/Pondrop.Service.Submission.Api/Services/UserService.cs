using Pondrop.Service.Submission.Application.Interfaces.Services;

namespace Pondrop.Service.Submission.Api.Services;

public class UserService : IUserService
{
    public string CurrentUserName() => "admin";
    public string GetMaterializedViewUserName() => "materialized_view";
}
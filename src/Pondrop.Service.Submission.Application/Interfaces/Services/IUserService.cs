namespace Pondrop.Service.Submission.Application.Interfaces.Services;

public interface IUserService
{
    string CurrentUserName();
    string GetMaterializedViewUserName();
}
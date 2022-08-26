using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;

namespace Pondrop.Service.Submission.Application.Commands.SubmissionTemplate.UpdateSubmissionTemplateCheckpointById;

public class UpdateSubmissionCheckpointByIdCommand : UpdateCheckpointByIdCommand<Result<SubmissionTemplateRecord>>
{
}
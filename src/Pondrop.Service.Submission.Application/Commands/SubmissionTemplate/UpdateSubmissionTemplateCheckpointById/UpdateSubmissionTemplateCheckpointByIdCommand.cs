using Pondrop.Service.Submission.Application.Commands;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;

namespace Pondrop.Service.Submission.Application.Commands;

public class UpdateSubmissionTemplateCheckpointByIdCommand : UpdateCheckpointByIdCommand<Result<SubmissionTemplateRecord>>
{
}
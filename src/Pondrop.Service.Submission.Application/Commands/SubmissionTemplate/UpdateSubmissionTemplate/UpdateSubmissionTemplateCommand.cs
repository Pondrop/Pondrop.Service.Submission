using MediatR;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Enums.SubmissionTemplate;
using Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;

namespace Pondrop.Service.SubmissionTemplate.Application.Commands;

public class UpdateSubmissionTemplateCommand : IRequest<Result<SubmissionTemplateRecord>>
{
    public Guid Id { get; init; } = Guid.Empty;
    public string Title { get; init; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int IconCodePoint { get; set; } = int.MaxValue;

    public string IconFontFamily { get; set; } = string.Empty;

    public string CreatedBy { get; set; } = string.Empty;
    public SubmissionTemplateType Type { get; set; } = SubmissionTemplateType.unknown;
    public SubmissionTemplateStatus Status { get; set; } = SubmissionTemplateStatus.unknown;
    public SubmissionTemplateFocus Focus { get; set; } = SubmissionTemplateFocus.unknown;
    public bool? IsForManualSubmissions { get; set; } = null;
}

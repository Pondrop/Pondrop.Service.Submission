using MediatR;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Enums.SubmissionTemplate;
using Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;

namespace Pondrop.Service.Submission.Application.Commands.SubmissionTemplate.AddStepToSubmission;

public class CreateFieldCommand : IRequest<Result<FieldRecord>>
{
    public string Label { get; init; } = string.Empty;

    public bool Mandatory { get; init; } = false;

    public SubmissionFieldType FieldType { get; init; } = SubmissionFieldType.unknown;

    public SubmissionFieldItemType? ItemType { get; init; } = null;

    public int? MaxValue { get; init; } = null;

    public List<string?>? PickerValues { get; init; } = null;


}
using MediatR;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Enums.SubmissionTemplate;
using Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;

namespace Pondrop.Service.Submission.Application.Commands.SubmissionTemplate.AddStepToSubmission;

public class UpdateFieldCommand : IRequest<Result<FieldRecord>>
{
    public Guid Id { get; init; } = Guid.Empty;

    public string? Label { get; init; } = null;

    public bool? Mandatory { get; init; } = null;

    public SubmissionFieldType? FieldType { get; init; } = null;

    public SubmissionFieldStatus? FieldStatus { get; init; } = null;

    public SubmissionFieldItemType? ItemType { get; init; } = null;

    public int? MaxValue { get; init; } = null;

    public List<string?>? PickerValues { get; init; } = null;


}
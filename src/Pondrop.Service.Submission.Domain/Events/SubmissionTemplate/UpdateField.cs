using Pondrop.Service.Submission.Domain.Enums.SubmissionTemplate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pondrop.Service.Submission.Domain.Events.Field;
public record UpdateField(
    Guid Id,
    string Label,
    bool Mandatory,
    SubmissionFieldType FieldType,
    SubmissionFieldItemType? ItemType,
    int? MaxValue,
    List<string?>? PickerValues) : EventPayload;

       
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pondrop.Service.Submission.Domain.Models.Submission;
public record SubmissionFieldWithDetailsViewRecord(
    Guid Id,
    Guid TemplateFieldId,
    string Label,
    string Type,
    List<FieldValuesRecord> Values)
{
}

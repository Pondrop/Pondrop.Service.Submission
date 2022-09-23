using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pondrop.Service.Submission.Domain.Models.Submission;
public record SubmissionStepWithDetailsViewRecord(
    Guid Id,
    Guid TemplateStepId,
    string Title,
    List<SubmissionFieldWithDetailsViewRecord> Fields)
{
}

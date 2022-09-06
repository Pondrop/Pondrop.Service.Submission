using AutoMapper;
using Pondrop.Service.Submission.Domain.Models;
using Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;

namespace Pondrop.Service.Submission.Domain.Mapper;

public class SubmissionTemplateProfile : Profile
{
    public SubmissionTemplateProfile()
    {
        CreateMap<SubmissionTemplateEntity, SubmissionTemplateRecord>();
        CreateMap<SubmissionTemplateEntity, SubmissionTemplateViewRecord>();
    }
}

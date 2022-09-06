using AutoMapper;
using Pondrop.Service.Submission.Domain.Models.Submission;

namespace Pondrop.Service.Submission.Domain.Mapper;

public class SubmissionProfile : Profile
{
    public SubmissionProfile()
    {
        CreateMap<SubmissionEntity, SubmissionRecord>();
        CreateMap<SubmissionEntity, SubmissionViewRecord>();
    }
}

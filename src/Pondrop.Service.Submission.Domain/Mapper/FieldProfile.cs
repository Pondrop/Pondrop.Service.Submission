using AutoMapper;
using Pondrop.Service.Submission.Domain.Models;
using Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;

namespace Pondrop.Service.Submission.Domain.Mapper;

public class FieldProfile : Profile
{
    public FieldProfile()
    {
        CreateMap<FieldEntity, FieldRecord>();
    }
}

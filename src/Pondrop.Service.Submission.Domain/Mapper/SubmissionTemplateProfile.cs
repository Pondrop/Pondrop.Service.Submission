﻿using AutoMapper;
using Pondrop.Service.Submission.Domain.Models;
using Pondrop.Service.Submission.Domain.Models.SubmissionTemplate;

namespace Pondrop.Service.Submission.Domain.Mapper;

public class SubmissionProfile : Profile
{
    public SubmissionProfile()
    {
        CreateMap<SubmissionTemplateEntity, SubmissionTemplateRecord>();
        CreateMap<SubmissionTemplateEntity, SubmissionViewRecord>();
    }
}

using AutoMapper;
using Pondrop.Service.Submission.Domain.Models.Campaign;

namespace Pondrop.Service.Submission.Domain.Mapper;

public class CampaignProfile : Profile
{
    public CampaignProfile()
    {
        CreateMap<CampaignEntity, CampaignRecord>();
    }
}

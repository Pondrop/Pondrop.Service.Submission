using AutoMapper;
using Pondrop.Service.Submission.Domain.Models.StoreVisit;

namespace Pondrop.Service.Submission.Domain.Mapper;

public class StoreVisitProfile : Profile
{
    public StoreVisitProfile()
    {
        CreateMap<StoreVisitEntity, StoreVisitRecord>();
        CreateMap<StoreVisitEntity, StoreVisitViewRecord>();
    }
}

﻿using MediatR;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Models.StoreVisit;
using Pondrop.Service.Submission.Domain.Models.Submission;

namespace Pondrop.Service.Submission.Application.Queries.Submission.GetAllStoreVisits;

public class GetAllStoreVisitsQuery : IRequest<Result<List<StoreVisitRecord>>>
{
    public int Limit { get; set; }

    public int Offset { get; set; }

}
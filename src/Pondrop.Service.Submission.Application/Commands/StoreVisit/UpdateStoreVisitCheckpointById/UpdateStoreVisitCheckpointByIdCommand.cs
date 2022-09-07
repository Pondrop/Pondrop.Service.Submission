﻿using Pondrop.Service.Submission.Application.Commands;
using Pondrop.Service.Submission.Application.Models;
using Pondrop.Service.Submission.Domain.Models.StoreVisit;
using Pondrop.Service.Submission.Domain.Models.Submission;

namespace Pondrop.Service.Submission.Application.Commands;

public class UpdateStoreVisitCheckpointByIdCommand : UpdateCheckpointByIdCommand<Result<StoreVisitRecord>>
{
}
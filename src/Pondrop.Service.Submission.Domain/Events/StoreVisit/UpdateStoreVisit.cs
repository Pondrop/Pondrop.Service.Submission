using Pondrop.Service.Submission.Domain.Enums.StoreVisit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pondrop.Service.Submission.Domain.Events.StoreVisit;

public record UpdateStoreVisit(
    Guid Id,
    double Latitude,
    double Longitude,
    ShopModeStatus ShopModeStatus) : EventPayload;

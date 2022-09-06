using Pondrop.Service.Submission.Domain.Enums.StoreVisit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pondrop.Service.Submission.Domain.Events.StoreVisit;
public record CreateStoreVisit(
    Guid Id,
    Guid StoreId,
    Guid UserId,
    double Latitude,
    double Longitude,
    ShopModeStatus ShopModeStatus)
{
}

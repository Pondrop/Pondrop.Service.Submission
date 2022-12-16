using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pondrop.Service.Submission.Domain.Models.SuggestedFieldValue;
public record ApplicableSubmissionViewRecord(
Guid Id,
DateTime SubmittedUtc,
string SubmittedUtcDayOfWeek,
Guid StoreId,
string StoreName,
string StoreRetailerName,
double StoreLatitude,
double StoreLongitude,
int SubmissionToStoreDistance,
int StoreToSubmissionStoreDistance,
double SubmissionFieldValue,
string VerifiedPhoto,
double SubmitterRecentAccuracy,
int SubmitterRecentFrequency,
int FibonnacciValueDenominator
)
{
    public ApplicableSubmissionViewRecord() : this(
        Guid.Empty,
        DateTime.MinValue,
        string.Empty,
        Guid.Empty,
        string.Empty,
        string.Empty,
        0,
        0,
        0,
        0,
        0,
        string.Empty,
        0,
        0,
        0
       )
    {
    }
}
using Pondrop.Service.Product.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pondrop.Service.Submission.Domain.Models.SuggestedFieldValue;

public record SuggestedFieldValueViewRecord(
    Guid Id,
       Guid StoreId,
       string StoreName,
       string StoreRetailerName,
 double StoreLatitude,
 double StoreLongitude,
 string FocusProductId,
 string FocusProductName,
 string FocusProductEAN,
 string FocusProductSize,
 string FocusProductBrand,
 List<CategoryViewRecord> FocusProductCategories,
 CategoryViewRecord FocusParentCategory,
 Guid FieldId,
 string? FieldLabel,
 string? FieldType,
 string? FieldItemType,
 int? FieldMaxValue,
 List<string?>? FieldPickerValues,
 double? SuggestedFieldValue,
double? FieldValueConfidence,
 List<ApplicableSubmissionViewRecord> ApplicableSubmissions
    )
{
    public SuggestedFieldValueViewRecord() : this(
        Guid.Empty,
        Guid.Empty,
        string.Empty,
        string.Empty,
        0,
        0,
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        new List<CategoryViewRecord>(),
        new CategoryViewRecord(),
        Guid.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        0,
        new List<string?>(),
        0,
        0,
        new List<ApplicableSubmissionViewRecord>()
       )
    {
    }
}
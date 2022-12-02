
using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.Submission.Domain.Models.Product;


public record ProductViewRecord(
      Guid Id,
        Guid? ParentCategoryId,
        string Name,
    Guid BrandId,
    string ExternalReferenceId,
    string Variant,
    string AltName,
    string ShortDescription,
    double NetContent,
    string NetContentUom,
    string PossibleCategories,
    string PublicationLifecycleId,
    List<Guid> ChildProductId,
    string? BarcodeNumber,
    string CategoryNames,
    CategoryViewRecord? ParentCategory,
    List<CategoryViewRecord>? Categories,
    DateTime? UpdatedUtc)
{
    public ProductViewRecord() : this(
          Guid.Empty,
        Guid.Empty,
        string.Empty,
        Guid.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        0,
        string.Empty,
        string.Empty,
        string.Empty,
        null,
        string.Empty,
        string.Empty,
        null,
        null,
        null)
    {
    }
}

namespace Pondrop.Service.Submission.Domain.Models.Product;

public record ProductViewRecord(
      Guid Id,
      string Name,
      string? BarcodeNumber)
{
    public ProductViewRecord() : this(
          Guid.Empty,
        string.Empty,
        string.Empty)
    {
    }
}
namespace Pondrop.Service.Product.Domain.Models;

public record CategoryViewRecord(
        Guid Id,
        string Name,
        string Type)
{
    public CategoryViewRecord() : this(
        Guid.Empty,
        string.Empty,
        string.Empty)
    {
    }
}
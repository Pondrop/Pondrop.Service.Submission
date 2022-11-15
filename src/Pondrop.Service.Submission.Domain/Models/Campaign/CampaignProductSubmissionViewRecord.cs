namespace Pondrop.Service.Submission.Domain.Models.Campaign;
public record CampaignProductSubmissionViewRecord(
       Guid SubmissionId,
       Guid CampaignId,
       Guid? UserId,
       Guid StoreId,
    Guid FocusProductId,
    string FocusProductName,
       string StoreName,
       string? Aisle,
       string? Section,
       string? Shelf,
       int? Quantity,
       string? NearestUseByDate,
       string? Issue,
       string? Comments)
{
    public CampaignProductSubmissionViewRecord() : this(
        Guid.Empty,
        Guid.Empty,
        Guid.Empty,
        Guid.Empty,
        Guid.Empty,
        string.Empty,
        string.Empty,
        null,
        null,
        null,
        null,
        null,
        null,
        null)
    {
    }
}
namespace Pondrop.Service.Submission.Domain.Models.Campaign;
public record CampaignSubmissionViewRecord(
       Guid SubmissionId,
       Guid CampaignId,
       Guid? UserId,
       Guid StoreId,
       string StoreName,
       string? Aisle,
       string? Section,
       string? Shelf,
       List<Guid>? Products,
       string? Issue,
       string? Comments)
{
    public CampaignSubmissionViewRecord() : this(
        Guid.Empty,
        Guid.Empty,
        Guid.Empty,
        Guid.Empty,
        string.Empty,
        null,
        null,
        null,
        null,
        null,
        null)
    {
    }
}
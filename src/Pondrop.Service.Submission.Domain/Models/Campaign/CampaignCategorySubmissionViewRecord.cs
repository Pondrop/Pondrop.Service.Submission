namespace Pondrop.Service.Submission.Domain.Models.Campaign;
public record CampaignCategorySubmissionViewRecord(
       Guid SubmissionId,
       Guid CampaignId,
       Guid? UserId,
       Guid StoreId,
    Guid FocusCategoryId,
    string FocusCategoryName,
       string StoreName,
       string? Aisle,
       string? Section,
       string? Shelf,
       List<Guid>? Products,
       string? Issue,
       string? Comments)
{
    public CampaignCategorySubmissionViewRecord() : this(
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
        null)
    {
    }
}
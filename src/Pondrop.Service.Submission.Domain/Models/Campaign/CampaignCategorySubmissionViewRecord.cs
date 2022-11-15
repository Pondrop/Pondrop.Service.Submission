namespace Pondrop.Service.Submission.Domain.Models.Campaign;

public record CampaignCategorySubmissionViewRecord(
    Guid FocusCategoryId,
    string FocusCategoryName) : CampaignSubmissionViewRecord
{
    public CampaignCategorySubmissionViewRecord() : this(
        Guid.Empty,
        string.Empty)
    {
    }
}
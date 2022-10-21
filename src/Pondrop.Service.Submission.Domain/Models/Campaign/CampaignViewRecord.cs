using Pondrop.Service.Submission.Domain.Enums.Campaign;

namespace Pondrop.Service.Submission.Domain.Models.Campaign;
public record CampaignViewRecord(
        Guid Id,
        string Name,
        string CampaignType,
        string SelectedTemplateTitle,
        int NumberOfStores,
        int Completions,
        DateTime? CampaignPublishedDate,
        string CampaignStatus)
{
    public CampaignViewRecord() : this(
        Guid.Empty,
        string.Empty,
        null,
        string.Empty,
        0,
        0,
        DateTime.MinValue,
        null)
    {
    }
}

namespace Pondrop.Service.Submission.Application.Models;

public class CampaignProductSubmissionFieldConfig
{
    public const string Key = nameof(CampaignProductSubmissionFieldConfig);
    
    public Guid ProductFocusFieldId { get; set; } = Guid.Empty;
    public Guid AisleFieldId { get; set; } = Guid.Empty;
    public Guid ShelfSectionFieldId { get; set; } = Guid.Empty;
    public Guid ShelfLabelFieldId { get; set; } = Guid.Empty;
    public Guid ProductPriceFieldId { get; set; } = Guid.Empty;
    public Guid QuantityFieldId { get; set; } = Guid.Empty;
    public Guid NearestUseByDateFieldId { get; set; } = Guid.Empty;
    public Guid ShelfIssueFieldId { get; set; } = Guid.Empty;
    public Guid CommentsFieldId { get; set; } = Guid.Empty;
}

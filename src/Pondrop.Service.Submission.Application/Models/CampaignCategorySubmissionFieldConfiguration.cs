namespace Pondrop.Service.Submission.Application.Models;

public class CampaignCategorySubmissionFieldConfiguration
{
    public const string Key = nameof(CampaignCategorySubmissionFieldConfiguration);
    
    public Guid CategoryFocusFieldId { get; set; } = Guid.Empty;
    public Guid AisleFieldId { get; set; } = Guid.Empty;
    public Guid ShelfSectionFieldId { get; set; } = Guid.Empty;
    public Guid ShelfLabelFieldId { get; set; } = Guid.Empty;
    public Guid ProductsFieldId { get; set; } = Guid.Empty;
    public Guid ShelfIssueFieldId { get; set; } = Guid.Empty;
    public Guid CommentsFieldId { get; set; } = Guid.Empty;
}

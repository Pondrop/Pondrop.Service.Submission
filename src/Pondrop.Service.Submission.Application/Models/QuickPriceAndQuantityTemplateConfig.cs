using Newtonsoft.Json;
using Pondrop.Service.Models;

namespace Pondrop.Service.Submission.Application.Models;

public class QuickPriceAndQuantityTemplateConfig
{
    public const string Key = nameof(QuickPriceAndQuantityTemplateConfig);
    
    public Guid Id { get; set; } = Guid.Empty;

    public Guid SearchProductFieldId { get; set; } = Guid.Empty;

    public Guid AisleFieldId { get; set; } = Guid.Empty;

    public Guid ShelfSectionFieldId { get; set; } = Guid.Empty;

    public Guid ShelfNumberFieldId { get; set; } = Guid.Empty;

    public Guid ShelfLabelFieldId { get; set; } = Guid.Empty;

    public Guid PhotoFieldId { get; set; } = Guid.Empty;

    public Guid ProductPriceFieldId { get; set; } = Guid.Empty;

    public Guid QuantityFieldId { get; set; } = Guid.Empty;

    public Guid ShelfIssueFieldId { get; set; } = Guid.Empty;

    public Guid CommentFieldId { get; set; } = Guid.Empty;
}

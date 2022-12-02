using Newtonsoft.Json;
using Pondrop.Service.Models;

namespace Pondrop.Service.Submission.Application.Models;

public class QuickPriceAndQuantityTemplateConfig
{
    public const string Key = nameof(QuickPriceAndQuantityTemplateConfig);
    public string? Id { get; set; }

    public string? SearchProductFieldId { get; set; }

    public string? AisleFieldId { get; set; }

    public string? ShelfSectionFieldId { get; set; }

    public string? ShelfNumberFieldId { get; set; }

    public string? ShelfLabelFieldId { get; set; }

    public string? PhotoFieldId { get; set; }

    public string? ProductPriceFieldId { get; set; }

    public string? QuanityFieldId { get; set; }

    public string? ShelfIssueFieldId { get; set; }

    public string? CommentFieldId { get; set; }
}

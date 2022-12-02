using Newtonsoft.Json;
using Pondrop.Service.Models;

namespace Pondrop.Service.Submission.Application.Models;

public class ShelfStockLevelsTemplateConfig
{
    public const string Key = nameof(ShelfStockLevelsTemplateConfig);
    public string? Id { get; set; }
    public string? SearchProductFieldId { get; set; }
    public string? PhotoFieldId { get; set; }
    public string? QuanityFieldId { get; set; }
    public string? AisleFieldId { get; set; }
    public string? ShelfSectionFieldId { get; set; }
    public string? ShelfNumberFieldId { get; set; }
    public string? NearestUseByDateFieldId { get; set; }
    public string? QuantityAtNearestUseByDateFieldId { get; set; }
    public string? FurthestUseByDateFieldId { get; set; }
    public string? QuantityAtFurthestUseByDateFieldId { get; set; }
    public string? CommentFieldId { get; set; }
}




using Newtonsoft.Json;
using Pondrop.Service.Models;

namespace Pondrop.Service.Submission.Application.Models;

public class ShelfStockLevelsTemplateConfig
{
    public const string Key = nameof(ShelfStockLevelsTemplateConfig);
    public Guid Id { get; set; } = Guid.Empty;
    public Guid SearchProductFieldId { get; set; } = Guid.Empty;
    public Guid PhotoFieldId { get; set; } = Guid.Empty;
    public Guid QuantityFieldId { get; set; } = Guid.Empty;
    public Guid AisleFieldId { get; set; } = Guid.Empty;
    public Guid ShelfSectionFieldId { get; set; } = Guid.Empty;
    public Guid ShelfNumberFieldId { get; set; } = Guid.Empty; 
    public Guid NearestUseByDateFieldId { get; set; } = Guid.Empty;
    public Guid QuantityAtNearestUseByDateFieldId { get; set; } = Guid.Empty;
    public Guid FurthestUseByDateFieldId { get; set; } = Guid.Empty;
    public Guid QuantityAtFurthestUseByDateFieldId { get; set; } = Guid.Empty;
    public Guid CommentFieldId { get; set; } = Guid.Empty;
}




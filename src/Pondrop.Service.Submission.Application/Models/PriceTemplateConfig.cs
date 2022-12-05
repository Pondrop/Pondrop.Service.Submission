using Newtonsoft.Json;
using Pondrop.Service.Models;

namespace Pondrop.Service.Submission.Application.Models;

public class PriceTemplateConfig
{
    public const string Key = nameof(PriceTemplateConfig);
    
    public Guid Id { get; set; } = Guid.Empty;
    public Guid SearchProductFieldId { get; set; } = Guid.Empty;
    public Guid PhotoFieldId { get; set; } = Guid.Empty;
    public Guid ProductPriceFieldId { get; set; } = Guid.Empty;
    public Guid PriceTypeFieldId { get; set; } = Guid.Empty;
    public Guid LabelProductNameFieldId { get; set; } = Guid.Empty;
    public Guid UnitPriceFieldId { get; set; } = Guid.Empty;
    public Guid UnitPriceUOMFieldId { get; set; } = Guid.Empty;
    public Guid LabelBarcodeFieldId { get; set; } = Guid.Empty;
    public Guid CommentFieldId { get; set; } = Guid.Empty;
}


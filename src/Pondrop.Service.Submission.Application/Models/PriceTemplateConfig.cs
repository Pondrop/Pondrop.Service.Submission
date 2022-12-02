using Newtonsoft.Json;
using Pondrop.Service.Models;

namespace Pondrop.Service.Submission.Application.Models;

public class PriceTemplateConfig
{
    public const string Key = nameof(PriceTemplateConfig);
    public string Id { get; set; }
    public string? SearchProductFieldId { get; set; }
    public string? PhotoFieldId { get; set; }
    public string? ProductPriceFieldId { get; set; }
    public string? PriceTypeFieldId { get; set; }
    public string? LabelProductNameFieldId { get; set; }
    public string? UnitPriceFieldId { get; set; }
    public string? UnitPriceUOMFieldId { get; set; }
    public string? LabelBarcodeFieldId { get; set; }
    public string? CommentFieldId { get; set; }
}


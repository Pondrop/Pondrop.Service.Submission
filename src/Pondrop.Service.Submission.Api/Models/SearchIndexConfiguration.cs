namespace Pondrop.Service.Submission.Api.Models;

public class SearchIndexConfiguration
{
    public const string Key = nameof(SearchIndexConfiguration);
    public string BaseUrl { get; set; } = string.Empty;

    public string ApiKey { get; set; } = string.Empty;

    public string ManagementKey { get; set; } = string.Empty;

    public string SubmissionIndexName { get; set; } = string.Empty;

    public string SubmissionIndexerName { get; set; } = string.Empty;

    public string SubmissionTemplateIndexName { get; set; } = string.Empty;

    public string SubmissionTemplateIndexerName { get; set; } = string.Empty;

    public string CampaignIndexName { get; set; } = string.Empty;

    public string CampaignIndexerName { get; set; } = string.Empty;
}

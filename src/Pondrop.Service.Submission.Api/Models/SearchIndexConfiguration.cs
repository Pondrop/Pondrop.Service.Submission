﻿namespace Pondrop.Service.Submission.Api.Models;

public class SearchIndexConfiguration
{
    public const string Key = nameof(SearchIndexConfiguration);
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string SubmissionIndexName { get; set; } = string.Empty;
}
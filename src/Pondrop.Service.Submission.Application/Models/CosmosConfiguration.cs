﻿using Microsoft.Extensions.Options;

namespace Pondrop.Service.Submission.Application.Models;

public class CosmosConfiguration 
{
    public const string Key = nameof(CosmosConfiguration);

    public string DatabaseName { get; set; } = string.Empty;
    public string ApplicationName { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
}

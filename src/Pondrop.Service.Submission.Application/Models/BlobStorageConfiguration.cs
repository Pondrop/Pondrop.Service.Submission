namespace Pondrop.Service.Submission.Application.Models;

public class BlobStorageConfiguration
{
    public const string Key = nameof(BlobStorageConfiguration);

    public string ContainerName { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
}

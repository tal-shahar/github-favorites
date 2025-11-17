using RepositoryAnalysisWorker.Models;

namespace RepositoryAnalysisWorker.Services.Contracts;

public interface IGitHubMetadataService
{
    Task<RepositoryMetadata> FetchAsync(string owner, string name, CancellationToken cancellationToken);
}


using RepositoryAnalysisWorker.Entities;
using RepositoryAnalysisWorker.Models;

namespace RepositoryAnalysisWorker.Services.Contracts;

public interface IRepositoryAnalysisService
{
    Task<RepositoryAnalysis> UpsertAnalysisAsync(Guid favoriteId, RepositoryMetadata metadata, CancellationToken cancellationToken);
}


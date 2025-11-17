FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY GitHubFavoritesWorkers/RepositoryAnalysisWorker/RepositoryAnalysisWorker.csproj GitHubFavoritesWorkers/RepositoryAnalysisWorker/
RUN dotnet restore GitHubFavoritesWorkers/RepositoryAnalysisWorker/RepositoryAnalysisWorker.csproj
COPY GitHubFavoritesWorkers/ GitHubFavoritesWorkers/
WORKDIR /src/GitHubFavoritesWorkers/RepositoryAnalysisWorker
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "RepositoryAnalysisWorker.dll"]


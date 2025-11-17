FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY GitHubFavoritesAPI/FavoritesAPI/FavoritesAPI.csproj GitHubFavoritesAPI/FavoritesAPI/
RUN dotnet restore GitHubFavoritesAPI/FavoritesAPI/FavoritesAPI.csproj
COPY GitHubFavoritesAPI/ GitHubFavoritesAPI/
WORKDIR /src/GitHubFavoritesAPI/FavoritesAPI
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "FavoritesAPI.dll"]


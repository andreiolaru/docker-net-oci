FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Directory.Build.props Directory.Packages.props ./
COPY src/DockerNetOci.Api/DockerNetOci.Api.csproj src/DockerNetOci.Api/
RUN dotnet restore src/DockerNetOci.Api/DockerNetOci.Api.csproj

COPY src/DockerNetOci.Api/ src/DockerNetOci.Api/
RUN dotnet publish src/DockerNetOci.Api/DockerNetOci.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

RUN groupadd -r appuser && useradd -r -g appuser -s /sbin/nologin appuser

WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .

USER appuser
ENTRYPOINT ["dotnet", "DockerNetOci.Api.dll"]

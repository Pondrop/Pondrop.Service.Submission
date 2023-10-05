#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/Pondrop.Service.Submission.Api/Pondrop.Service.Submission.Api.csproj", "src/Pondrop.Service.Submission.Api/"]
COPY ["src/Pondrop.Service.Submission.Application/Pondrop.Service.Submission.Application.csproj", "src/Pondrop.Service.Submission.Application/"]
COPY ["src/Pondrop.Service.Submission.Domain/Pondrop.Service.Submission.Domain.csproj", "src/Pondrop.Service.Submission.Domain/"]
COPY ["src/Pondrop.Service.Submission.Infrastructure/Pondrop.Service.Submission.Infrastructure.csproj", "src/Pondrop.Service.Submission.Infrastructure/"]
RUN dotnet nuget add source "https://pkgs.dev.azure.com/PondropDevOps/_packaging/PondropDevOps/nuget/v3/index.json" --name "PondropInfrastructure" --username "user" --password "qylafrpgp6sxjwuvmipxajoq3yh5qwg3d5encx27mom5bmn3naza" --store-password-in-clear-text
RUN dotnet restore "src/Pondrop.Service.Submission.Api/Pondrop.Service.Submission.Api.csproj"
COPY . .
WORKDIR "/src/src/Pondrop.Service.Submission.Api"
RUN dotnet build "Pondrop.Service.Submission.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Pondrop.Service.Submission.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Pondrop.Service.Submission.Api.dll"]
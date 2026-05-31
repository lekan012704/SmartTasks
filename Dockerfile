# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and restore
COPY *.sln .
COPY SmartTask.Api/SmartTask.Api.csproj SmartTask.Api/
COPY SmartTask.Application/SmartTask.Application.csproj SmartTask.Application/
COPY SmartTask.Domain/SmartTask.Domain.csproj SmartTask.Domain/
COPY SmartTask.Persistence/SmartTask.Persistence.csproj SmartTask.Persistence/
COPY SmartTask.Identity/SmartTask.Identity.csproj SmartTask.Identity/
COPY SmartTask.Shared/SmartTask.Shared.csproj SmartTask.Shared/

RUN dotnet restore

# Copy everything and build
COPY . .
WORKDIR /src/SmartTask.Api
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Render injects PORT — default to 10000
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "SmartTask.Api.dll"]
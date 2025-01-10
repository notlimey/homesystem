FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY *.sln .
COPY ["*.csproj", "./"]
RUN dotnet restore

# Copy everything else
COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Add healthcheck
HEALTHCHECK --interval=30s --timeout=3s \
    CMD curl -f http://localhost/health || exit 1

EXPOSE 80
ENTRYPOINT ["dotnet", "YourApiName.dll"]

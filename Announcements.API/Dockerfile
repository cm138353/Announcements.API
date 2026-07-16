FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .

# Remove any Windows-generated build artifacts
RUN find . -type d \( -name bin -o -name obj \) -prune -exec rm -rf {} +

RUN dotnet restore "Announcements.API/Announcements.API.csproj" \
    -p:RestoreFallbackFolders=""

RUN dotnet publish "Announcements.API/Announcements.API.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Announcements.API.dll"]
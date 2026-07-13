FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/backend/Directory.Build.props .
COPY src/backend/OrgSphere.Domain/OrgSphere.Domain.csproj src/backend/OrgSphere.Domain/
COPY src/backend/OrgSphere.Application/OrgSphere.Application.csproj src/backend/OrgSphere.Application/
COPY src/backend/OrgSphere.Infrastructure/OrgSphere.Infrastructure.csproj src/backend/OrgSphere.Infrastructure/
COPY src/backend/OrgSphere.API/OrgSphere.API.csproj src/backend/OrgSphere.API/
RUN dotnet restore src/backend/OrgSphere.API/OrgSphere.API.csproj

COPY src/backend/ src/backend/
RUN dotnet publish src/backend/OrgSphere.API/OrgSphere.API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OrgSphere.API.dll"]

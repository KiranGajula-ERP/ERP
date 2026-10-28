# AuditFlow .NET 10 ASP.NET Core application container
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY AuditFlow.Web.csproj ./
RUN dotnet restore AuditFlow.Web.csproj
COPY . ./
RUN dotnet publish AuditFlow.Web.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    Web__EnableHttpsRedirection=false \
    Storage__RootPath=/app/media
COPY --from=build /app/publish ./
RUN mkdir -p /app/media
EXPOSE 8080
ENTRYPOINT ["dotnet", "AuditFlow.Web.dll"]

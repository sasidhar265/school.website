FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore src/SchoolConnect.Web/SchoolConnect.Web/SchoolConnect.Web.csproj
RUN dotnet publish src/SchoolConnect.Web/SchoolConnect.Web/SchoolConnect.Web.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV PORT=10000
EXPOSE 10000

USER app
ENTRYPOINT ["dotnet", "SchoolConnect.Web.dll"]

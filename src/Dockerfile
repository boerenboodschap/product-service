FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS publish
WORKDIR /src
COPY . .
RUN dotnet restore /ignoreprojectextensions:.dcproj
WORKDIR /src
RUN dotnet publish product-service.csproj -c Release -o /app

FROM base AS final
WORKDIR  /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "product-service.dll"]

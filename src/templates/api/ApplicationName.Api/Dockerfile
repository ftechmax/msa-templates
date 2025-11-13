FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV COMPlus_EnableDiagnostics=0
RUN addgroup --gid 20000 dotnet && adduser --uid 10000 --disabled-password --ingroup dotnet dotnet
ENV DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE=false

FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src
COPY . .
WORKDIR "/src/api/ApplicationName.Api"
RUN dotnet publish "ApplicationName.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build --chown=10000:20000 /app/publish .

USER 10000
ENTRYPOINT ["dotnet", "ApplicationName.Api.dll"]
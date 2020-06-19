FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
RUN apt-get update \
    && apt-get install -y --allow-unauthenticated \
        libc6-dev \
        libgdiplus \
        libx11-dev \
     && rm -rf /var/lib/apt/lists/*
#FROM mcr.microsoft.com/dotnet/core/aspnet:2.2 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src

COPY ["./shared", "shared"]
COPY ["./UmkSvc/umk-svc", "UmkSvc/umk-svc"]
COPY ["./UserGroupManager/ug-mngt", "UserGroupManager/ug-mngt"]
COPY ["./UmkClasses/umk-classes", "UmkClasses/umk-classes"]
COPY ["./UmkMigrate/umk-migrate", "UmkMigrate/umk-migrate"]
COPY ["./snk", "snk"]

RUN dotnet restore "UmkSvc/umk-svc/umk-svc.csproj"

RUN dotnet build /nowarn:CS8002 "UmkSvc/umk-svc/umk-svc.csproj" -o /app

FROM build AS publish
RUN dotnet publish /nowarn:CS8002 "UmkSvc/umk-svc/umk-svc.csproj" -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "umk-svc.dll"]
#ENTRYPOINT ["/bin/bash"]

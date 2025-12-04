FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env

WORKDIR /App

# Copy everything
COPY . ./

# Create configuration files for dev and production. Else docker build will fail
# At runtime, all the configuration variables can be superseeded by env vars. 
RUN cp src/Ddi.Registry.ZoneWriter/zonesettings.json.dist src/Ddi.Registry.ZoneWriter/zonesettings.Development.json \
    && cp src/Ddi.Registry.Web/appsettings.json.dist src/Ddi.Registry.Web/appsettings.Development.json \
    && cp src/Ddi.Registry.ZoneWriter/zonesettings.json.dist src/Ddi.Registry.ZoneWriter/zonesettings.json \
    && cp src/Ddi.Registry.Web/appsettings.json.dist src/Ddi.Registry.Web/appsettings.json 

# Restore as distinct layers
RUN dotnet restore ./Ddi.Registry.Web.sln
# Build and publish a release
RUN dotnet build ./Ddi.Registry.Web.sln -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /App
COPY --from=build-env /App/out .

ENTRYPOINT ["dotnet", "Ddi.Registry.Web.dll"]

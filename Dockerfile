# Use the official .NET SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0@sha256:faa2daf2b72cbe787ee1882d9651fa4ef3e938ee56792b8324516f5a448f3abe AS build
WORKDIR /src

# Copy project files
COPY *.csproj .
RUN dotnet restore
COPY . .
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish -c Release -o /app

# Use the ASP.NET runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0@sha256:1e12c265e1e1b3714c5805ab0cab63380eb687b0a04f3b3ef3392494a6122614 AS runtime
WORKDIR /app
COPY --from=build /app .

# Ensure the UserList.json file is available
COPY UserList.json ./UserList.json

# Expose port 80 (or 5000 if you prefer)
EXPOSE 80

# Set the entry point
ENTRYPOINT ["dotnet", "blockChainApp.dll"]


# Use the official .NET 8 SDK image for the build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory in the container
WORKDIR /app

# Copy the .csproj and solution files to the container
COPY IDPServer.sln ./
COPY IDPServer/IDPServer.csproj IDPServer/

# Restore dependencies
RUN dotnet restore IDPServer/IDPServer.csproj

# Copy the rest of the application files
COPY . .

# Build the project
RUN dotnet build IDPServer/IDPServer.csproj -c Release -o /app/build

# Publish the project
RUN dotnet publish IDPServer/IDPServer.csproj -c Release -o /app/publish

# Use the official .NET 8 runtime image for the runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Set the working directory in the container
WORKDIR /app

# Copy the published files from the build stage
COPY --from=build /app/publish .

# Expose the port that the application will run on
EXPOSE 80

# Set the entry point for the container
ENTRYPOINT ["dotnet", "IDPServer.dll"]

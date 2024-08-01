# Use the official ASP.NET Core SDK image for building the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory inside the container
WORKDIR /app

# Copy the .csproj file and restore dependencies
COPY IDPServer.csproj . 
RUN dotnet restore

# Copy the rest of the application source code
COPY . . 

# Build the application in Release mode
RUN dotnet publish -c Release -o out

# Use the official ASP.NET Core runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Set the working directory inside the container
WORKDIR /app

# Copy the built application from the previous stage
COPY --from=build /app/out .

# Expose the port the application will run on
EXPOSE 80

# Set the entry point for the container to run the application
ENTRYPOINT ["dotnet", "IDPServer.dll"]

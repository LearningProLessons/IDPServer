# Use the official .NET image as the base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Use the SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the project files and restore the dependencies
COPY ["IDPServer.csproj", "./"]
RUN dotnet restore "IDPServer.csproj"

# Copy the remaining source code and build the application
COPY . .
RUN dotnet build "IDPServer.csproj" -c Release -o /app/build

# Install EF Core tools
RUN dotnet tool install --global dotnet-ef

# Ensure dotnet tools are in PATH
ENV PATH="${PATH}:/root/.dotnet/tools"

# Add migrations and update the database
RUN dotnet ef migrations add Users2 -c ApplicationDbContext -o Data/Migrations
RUN dotnet ef database update -c ApplicationDbContext --verbose

# Publish the application
FROM build AS publish
RUN dotnet publish "IDPServer.csproj" -c Release -o /app/publish

# Final stage to create the runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Copy and run the database setup and seeding scripts
COPY ./scripts /scripts

# Run the seed data script
ENTRYPOINT ["sh", "/scripts/setup.sh"]

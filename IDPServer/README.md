# IDPServer Deployment Guide

## Overview

This guide explains how to set up and run the `IDPServer` application using Docker. It includes instructions for both a simple Docker setup and a more advanced failover clustering setup with Nginx.

## Prerequisites

- **Docker**: Ensure Docker is installed and running on your machine.
- **Docker Compose**: Install Docker Compose for orchestrating multi-container applications.

## Folder Structure

Here's an overview of the folder structure:

```
IDPServer
├── docker-compose.yml       # Docker Compose configuration
├── nginx.conf               # Nginx configuration for load balancing
├── IDPServer                # .NET application directory
│   ├── Dockerfile           # Dockerfile for building .NET app image
│   ├── appsettings.json     # Application settings
│   ├── Config.cs            # Configuration class
│   ├── Data                 # Data-related classes
│   ├── HostingExtensions.cs # Hosting extensions
│   ├── IDPServer.csproj     # .NET project file
│   ├── Models               # Model classes
│   ├── Pages                # Razor pages
│   ├── Program.cs           # Application entry point
│   ├── Properties           # Project properties
│   ├── README.md            # This file
│   ├── SeedData.cs          # Seed data for initialization
│   ├── setup.ps1            # Setup script (optional)
│   ├── tempkey.jwk          # Temporary key for JWT (if applicable)
│   └── wwwroot              # Static files
└── IDPServer.sln            # Solution file
```

## Simple Docker Setup

### Building and Running the Application

1. **Navigate to the Project Directory**

   Change to the directory where the `Dockerfile` is located:

   ```bash
   cd IDPServer
   ```

2. **Build the Docker Image**

   Build the Docker image for the .NET application:

   ```bash
   docker build -t my-sso:v1.0.0 .
   ```

3. **Run the Docker Container**

   Run a container from the built image:

   ```bash
   docker run -d -p 8080:80 --name my-sso my-sso:v1.0.0
   ```

   Access the application by navigating to `http://localhost:8080`.

## Failover Clustering with Nginx

### Overview

In this setup, Nginx acts as a reverse proxy and load balancer, distributing traffic across multiple instances of the .NET application to ensure high availability.

### Setting Up

1. **Navigate to the Root Directory**

   Change to the directory where the `docker-compose.yml` and `nginx.conf` files are located:

   ```bash
   cd IDPServer
   ```

2. **Start the Cluster**

   Use Docker Compose to start the application and Nginx services:

   ```bash
   docker-compose up -d
   ```

   This command will build and start two instances of the .NET application and an Nginx container configured for load balancing.

3. **Access the Application**

   Open a web browser and navigate to `http://localhost:8080`. Nginx will distribute the requests between the available application instances.

### Configuration Details

- **Nginx Configuration (`nginx.conf`)**:

  ```nginx
  events {}

  http {
      upstream backend {
          server app1:80;
          server app2:80;
      }

      server {
          listen 80;

          location / {
              proxy_pass http://backend;
              proxy_set_header Host $host;
              proxy_set_header X-Real-IP $remote_addr;
              proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
              proxy_set_header X-Forwarded-Proto $scheme;
          }
      }
  }
  ```

- **Docker Compose Configuration (`docker-compose.yml`)**:

  ```yaml
  version: '3.8'

  services:
    app1:
      image: my-sso:v1.0.0
      container_name: app1
      restart: always
      networks:
        - mynetwork

    app2:
      image: my-sso:v1.0.0
      container_name: app2
      restart: always
      networks:
        - mynetwork

    nginx:
      image: nginx:latest
      container_name: nginx
      ports:
        - "8080:80"
      volumes:
        - ./nginx.conf:/etc/nginx/nginx.conf
      networks:
        - mynetwork

  networks:
    mynetwork:
      driver: bridge
  ```

### Notes

- **Scaling**: To scale the number of application instances, adjust the `docker-compose.yml` file and rerun `docker-compose up -d`.
- **Health Checks**: Implement health checks to monitor application health.
- **Security**: Consider securing the application and Nginx setup with HTTPS and appropriate firewall rules.

## Troubleshooting

- **Check Logs**: Use `docker logs <container_name>` to view logs if the containers are not behaving as expected.
- **Network Issues**: Ensure that all containers are on the same network and can communicate with each other.

For further customization or advanced configurations, refer to the Docker and Nginx documentation.

 
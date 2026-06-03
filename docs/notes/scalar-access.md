# 001 - Accessing Scalar API Documentation

**Date:** 2026-06-03  
**Status:** Active  
**Context:** The API documentation uses Scalar. Due to security best practices, the backend defaults to the `Production` environment in Docker, which disables OpenAPI and the Scalar UI. This note documents the necessary steps to bypass this behavior for local development and testing.

By default, the `network-monitoring-backend` Docker container runs in **Production** mode for security reasons. In this mode, the OpenAPI specifications and the Scalar UI are disabled.

To access the Scalar documentation locally, the application must be forced to run in the **Development** environment.

## Method 1: Permanent change in Docker Compose (Recommended for local dev)

1. Open the `docker-compose.reference-stack.yml` file.
2. Locate the `network-monitoring-backend` service.
3. Uncomment or add the `ASPNETCORE_ENVIRONMENT` variable in the `environment` section:

```yaml
  network-monitoring-backend:
    environment:
      # Uncomment or add the following line to enable Scalar API Documentation
      ASPNETCORE_ENVIRONMENT: Development
      ASPNETCORE_URLS: http://+:8080
```

4. Recreate the container:
```bash
docker-compose -f docker-compose.reference-stack.yml up -d network-monitoring-backend
```

5. Access Scalar in your browser:
👉 **http://localhost:5090/scalar/v1**

---

## Method 2: Ephemeral container via command line (No file changes)

If you don't want to modify the compose file, you can spin up a temporary container injecting the variable on the fly. 

*Note: You must stop the main backend container first to free up port 5090.*

```bash
# 1. Stop the running backend
docker-compose -f docker-compose.reference-stack.yml stop network-monitoring-backend

# 2. Run an ephemeral container with the Development flag
docker-compose -f docker-compose.reference-stack.yml run -d -e ASPNETCORE_ENVIRONMENT=Development -p 5090:8080 network-monitoring-backend
```

When you are done, clean up the ephemeral container and start the normal one:
```bash
docker-compose -f docker-compose.reference-stack.yml down --remove-orphans
docker-compose -f docker-compose.reference-stack.yml up -d network-monitoring-backend
```

## Troubleshooting
If you see a `404 Not Found` even when using the `Development` flag, ensure that the Docker image is compiled with the latest C# code. You might need to force a rebuild:
```bash
docker-compose -f docker-compose.reference-stack.yml build network-monitoring-backend
```

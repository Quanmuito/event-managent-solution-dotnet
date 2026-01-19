## Prerequisites

- .NET SDK 9.0 or later
- Docker Desktop (or Docker Engine + Docker Compose)
- IDE (Visual Studio, Rider, or VS Code with C# extension)

## Installation

1. Start the database services using Docker Compose:
```bash
docker compose up -d
```

This will start:
- MongoDB on port 27017
- Mongo Express (database UI) on port 8081

2. Restore NuGet packages:
```bash
dotnet restore
```

3. Build the solution:
```bash
dotnet build
```

4. Run the services:

**EventService API:**
```bash
cd EventService/EventService.Api
dotnet run
```

**BookingService API:**
```bash
cd BookingService/BookingService.Api
dotnet run
```

The APIs will be available at the ports configured in their respective `launchSettings.json` files (typically `http://localhost:5000` for EventService and `http://localhost:5001` for BookingService).

## Verify

1. Check MongoDB is running:
```bash
docker ps
```
You should see `database-service-container` and `mongo-express-container` running.

2. Access Mongo Express:
Open `http://localhost:8081` in your browser to view the database.

3. Test the API health checks:
```bash
curl http://localhost:5000/health
curl http://localhost:5001/health
```
Should return: `Healthy`

4. Check OpenAPI documentation (Development only):
- EventService: Visit `http://localhost:5000/openapi/v1.json`
- BookingService: Visit `http://localhost:5001/openapi/v1.json`

## Debug

### VS Code / Rider / Visual Studio

1. Set `EventService.Api` or `BookingService.Api` as the startup project
2. Ensure Docker services are running (`docker compose up -d`)
3. Set breakpoints and start debugging (F5)

**Note:** To run both services simultaneously, you can:
- Use multiple debug configurations
- Run one service in debug mode and the other from the terminal
- Use Docker Compose to orchestrate both services

### Common Issues

**MongoDB connection fails:**
- Verify Docker services are running: `docker compose ps`
- Check connection string in `appsettings.json` matches Docker service hostname (`mongo`)
- If running outside Docker, update connection string to `mongodb://localhost:27017`

**Port already in use:**
- Change ports in `launchSettings.json` or `compose.yaml`
- Stop conflicting services

**Build errors:**
- Run `dotnet clean` and `dotnet restore`
- Verify .NET SDK version: `dotnet --version` (should be 9.0+)

### Logs

Application logs are configured via Serilog. Check console output for log messages.

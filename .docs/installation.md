## Prerequisites

- .NET SDK 9.0
- Docker Desktop (or Docker Engine + Docker Compose)
- IDE (Visual Studio, Rider, or VS Code with C# extension)

## Installation

1. Start the infrastructure services using Docker Compose:
```bash
docker compose up -d
```

This will start:
- MongoDB (database-service) on port 27017
- Mongo Express (database UI) on port 8081
- LocalStack (AWS services emulator) on port 4566
- SES verifier (configures email verification in LocalStack)

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

The APIs will be available at:
- EventService: `http://localhost:5000`
- BookingService: `http://localhost:5001`

Ports are configured in `Properties/launchSettings.json` files for each API project.

## Verify

1. Check Docker services are running:
```bash
docker ps
```
You should see:
- `database-service-container` (MongoDB)
- `mongo-express-container` (Mongo Express)
- `localstack-container` (LocalStack)
- `ses-verifier-container` (SES verifier - runs once and exits)

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
- Default connection string: `mongodb://mongo:27017` (for Docker) or `mongodb://localhost:27017` (for local)
- Database name: `events-management-solution`
- Credentials: username `user123`, password `password123`

**Port already in use:**
- Change ports in `Properties/launchSettings.json` or `compose.yaml`
- Stop conflicting services

**Build errors:**
- Run `dotnet clean` and `dotnet restore`
- Verify .NET SDK version: `dotnet --version` (should be 9.0)

### Logs

Application logs are configured via Serilog. Check console output for log messages.

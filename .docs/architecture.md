# Architecture

## Dependency Overview

### Layer 0: Foundation (No Project Dependencies)

- **DatabaseService** - MongoDB infrastructure and repository pattern
- **AspNet.Common** - ASP.NET Core abstractions and extensions
- **AWSService** - AWS client factories and configuration
- **Ems.Common** - Shared business logic and cross-cutting concerns (logging, task processing, exception handling)

### Layer 1: Infrastructure Services

- **NotificationService.Common** → AWSService, Ems.Common
- **NotificationService.Data** → DatabaseService
- **EventService.Data** → DatabaseService
- **BookingService.Data** → DatabaseService

### Layer 2: API Services

- **EventService.Api** → AspNet.Common, Ems.Common, DatabaseService, EventService.Data
  - Indirect dependencies: None (all direct dependencies listed)
- **BookingService.Api** → AspNet.Common, Ems.Common, DatabaseService, AWSService, NotificationService.Common, EventService.Data, BookingService.Data
  - Indirect dependencies: None (all direct dependencies listed)

## Dependency Layers

1. **Layer 0 (Foundation)**: DatabaseService, AspNet.Common, AWSService, Ems.Common

   - Base infrastructure with no project dependencies
   - `Ems.Common` provides shared business logic and cross-cutting concerns (logging, task processing, exception handling)
   - `DatabaseService` provides MongoDB infrastructure and generic repository pattern
   - `AspNet.Common` provides API versioning and common endpoint mappings
   - `AWSService` provides AWS client factories and configuration

2. **Layer 1 (Infrastructure Services)**: NotificationService.Common, NotificationService.Data, EventService.Data, BookingService.Data

   - Services and data access layers depending on foundation
   - `NotificationService.Common` depends on `AWSService` and `Ems.Common` from Layer 0
   - Data layers (`NotificationService.Data`, `EventService.Data`, `BookingService.Data`) depend on `DatabaseService` from Layer 0
   - Provides reusable infrastructure that can be consumed by multiple API services

3. **Layer 2 (API Services)**: EventService.Api, BookingService.Api

   - Application entry points consuming all lower layers
   - `EventService.Api` depends on Layer 0 and `EventService.Data` from Layer 1
   - `BookingService.Api` depends on Layer 0, Layer 1 services, and `EventService.Data` (for event validation)
   - Each API service is independently deployable and runs on separate portss

## Architecture Details

This is a microservices-based event management system built with .NET 9.0, following a layered architecture pattern with clear separation of concerns.

### Technology Stack

- **.NET 9.0**: Framework for all services
- **MongoDB 8.0**: Document database for data persistence
- **Docker Compose**: Container orchestration for infrastructure services
- **LocalStack**: AWS services emulator (SES) for local development
- **Serilog**: Structured logging framework
- **QRCoder**: QR code generation library

### Service Ports

- **EventService.Api**: `http://localhost:5000`
- **BookingService.Api**: `http://localhost:5001`
- **MongoDB**: `localhost:27017`
- **Mongo Express**: `http://localhost:8081`
- **LocalStack**: `http://localhost:4566`

### Data Flow

1. **API Layer**: Controllers receive HTTP requests with API versioning (`/v1/events`, `/v1/bookings`)
2. **Service Layer**: Business logic services handle DTO mapping, validation, and orchestration
3. **Task Queueing**: Background tasks are enqueued via `ITaskQueue<T>` for asynchronous processing:
   - QR code generation tasks (`QrCodeTaskMessage`)
   - Email notification tasks (`EmailNotificationTaskMessage<BookingDto>`)
   - Phone notification tasks (`PhoneNotificationTaskMessage<BookingDto>`)
4. **Task Processing**: Background task processors (hosted services) consume queued messages:
   - **QrCodeTaskProcessor**: Generates QR codes using QRCoder and stores them via `IQrCodeRepository`
   - **BookingEmailNotificationTaskProcessor**: Extends `EmailNotificationTaskProcessor`, sends email notifications with booking and event details via `IEmailService`
   - **BookingPhoneNotificationTaskProcessor**: Extends `PhoneNotificationTaskProcessor`, sends phone notifications via `IPhoneService`
5. **Repository Layer**: Repositories interact with `DatabaseService` using the generic repository pattern
6. **Database Layer**: `DatabaseService` manages MongoDB connections, provides `MongoDbContext`, and implements generic CRUD operations
7. **External Services**: Notification services use AWS SES (via LocalStack in development) for sending emails

### Core Services

- **DatabaseService**: Provides MongoDB infrastructure including:
  - `MongoDbContext`: Singleton service managing MongoDB client and database connections
  - Generic `Repository<T>` base class with CRUD operations (`GetByIdAsync`, `GetAllAsync`, `CreateAsync`, `UpdateAsync`, `DeleteAsync`)
  - `MongoDbSettings`: Configuration for connection strings and database names
  - Custom exceptions for entity not found scenarios

### Common Libraries

- **Ems.Common**: Provides shared cross-cutting concerns:
  - Logging extensions using Serilog with console output (configurable by environment)
  - Background task processing infrastructure (`ITaskQueue<T>`, `ITaskProcessor<T>`, `TaskService<T>`)
  - HTTP exception handling with `GlobalExceptionHandler`
  - Error response models (`ErrorResponse`, `ModelStateErrorResponse`, `IdRequiredErrorResponse`, etc.)
  - Task message types for notifications (`EmailNotificationTaskMessage<T>`, `PhoneNotificationTaskMessage<T>`)
  - HTTP client abstractions for inter-service communication

- **AspNet.Common**: Provides ASP.NET Core abstractions:
  - API versioning configuration and extensions
  - Common API endpoint mappings (`MapCommonApiEndpoints`) including health checks (`/health`) and OpenAPI (`/openapi/v1.json` in development)
  - Service collection extensions for API setup

### Infrastructure Services

- **AWSService**: Provides AWS integration:
  - AWS SES client factory for email sending
  - AWS settings configuration (`AWSSettings`, `AWSSESSettings`)
  - Service collection extensions for AWS service registration
  - Supports LocalStack endpoint configuration for local development

- **NotificationService**: Provides notification infrastructure:
  - **NotificationService.Common**: Shared notification infrastructure
    - `IEmailService` and `EmailService`: Email sending via AWS SES
    - `IPhoneService` and `PhoneService`: Phone/SMS notification services
    - Base task processors (`EmailNotificationTaskProcessor<T>`, `PhoneNotificationTaskProcessor<T>`) that can be extended by consuming services
    - Task message types (`EmailNotificationTaskMessage<T>`, `PhoneNotificationTaskMessage<T>`)
    - Depends on `AWSService` for AWS SES integration and `Ems.Common` for task processing infrastructure
  - **NotificationService.Data**: Data access layer (prepared for future use)
  - **NotificationService.Api**: API layer (prepared for future use)

### Microservices

- **EventService**: Manages event lifecycle:
  - **EventService.Api**: RESTful API with versioning (`/v1/events`)
    - Endpoints: Search, GetById, Create, Update, Delete
    - Search supports comma-separated keywords with case-insensitive matching in title and details
    - Uses `HandleEventService` for business logic
  - **EventService.Data**: Data access layer
    - `Event` model with MongoDB attributes
    - `IEventRepository` and `EventRepository` with custom search functionality
    - Extends generic `Repository<Event>` from DatabaseService

- **BookingService**: Manages booking lifecycle with integrated notifications:
  - **BookingService.Api**: RESTful API with versioning (`/v1/bookings`)
    - Endpoints: GetById, Create, Update, Confirm, Cancel, Delete
    - Validates event existence before creating bookings
    - Uses `HandleBookingService` for business logic
    - Integrates with EventService.Data to fetch event details for notifications
  - **BookingService.Data**: Data access layer
    - `Booking` model with status management (`registered`, `canceled`, `queue_enrolled`, `queue_pending`)
    - `IBookingRepository` and `BookingRepository`
    - `IQrCodeRepository` and `QrCodeRepository` for QR code storage
  - **Background Task Processors** (three hosted services):
    - **QrCodeTaskProcessor**: Generates QR codes using QRCoder library, stores via `IQrCodeRepository`
    - **BookingEmailNotificationTaskProcessor**: Extends `EmailNotificationTaskProcessor<BookingDto>`, sends email notifications with booking and event details via `IEmailService`
    - **BookingPhoneNotificationTaskProcessor**: Extends `PhoneNotificationTaskProcessor<BookingDto>`, sends phone notifications via `IPhoneService`

### External Services

- **MongoDB 8.0**: Document database for data persistence
  - Database: `events-management-solution`
  - Collections: `Events`, `Bookings`, `QrCodes`
  - Authentication: username `user123`, password `password123` (development)

- **LocalStack**: AWS services emulator for local development
  - SES service emulation on port 4566
  - Email retrieval endpoint: `http://localhost:4566/_aws/ses`
  - Configured via `compose.yaml` with SES verifier container

- **Mongo Express**: Web-based MongoDB administration interface on port 8081

## Configuration

### Environment Variables

Both API services support environment variable configuration with prefixes:
- **EventService**: `EventService_*` prefix
- **BookingService**: `BookingService_*` prefix

### Configuration Sections

- **MongoDb**: Connection string and database name
- **AWS**: AWS credentials and endpoint configuration (LocalStack endpoint for development)

## Error Handling

- Global exception handler (`GlobalExceptionHandler`) catches and formats exceptions
- Standardized error response models for consistent API error formatting
- Repository layer throws `KeyNotFoundException` for entity not found scenarios
- Validation errors return `ModelStateErrorResponse` with detailed field-level errors
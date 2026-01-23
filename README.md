## Event Management System

### Description

This is an on-going project for learning purpose to get an understand on <strong>microservices</strong> and learn to use <strong>C#</strong> with <strong>.NET Core</strong>.

### User Stories

1. As a user, I want to browse upcoming events, so I can find events to attend.
2. As a user, I want to search for events by name, date, or location, so I can quickly find specific events.
3. As a user, I want to view detailed information about an event, so I can decide if I want to attend.
4. As a user, I want to book a spot for event, If there are no spaces anymore, I want to  enroll to a queue and get a spot later, if any
5. As a user, I want to receive notifications about event updates, so I stay informed
6. As an event organizer, I want to create and manage events

### Requirements

- User Authentication: Users must be able to register (we will not implement the registration part, but assume it still needs to be done), log in, and manage their events or attendances.
- Event Catalog: The system must store and manage event details, including name, date, location, description, and availability.
- Search and Filter: Users must be able to search and filter events by various criteria.
- Ticket generation: Users must be able to get a valid ticket to present offline (e.g. QR codes)
- Notifications: Users must receive notifications about event updates.
- Organizer Panel: Event organizers must have access to create and manage events.

### Business Flows

1. Browsing and Searching Events:
    - User navigates to the event management homepage.
    - User browses events by category or uses the search bar to find specific events.
    - System retrieves event data from a database and displays results.
2. Viewing Event Details:
    - User clicks on an event to view its details.
    - System retrieves detailed event information from a database and displays it.
3. Booking a space:
    - User selects an event and enrolls to the attendance
    - System processes the event and creates an a QR code for the user
    - System sends booking confirmation and event updates
4. Enrolling to a queue:
    - If there is no available spaces on an event, a user can enroll to a queue
    - If somebody left the queue, first person from the queue should get a space, and needs to confirm his attendance, otherwise a next person will get the space and so on
5. Managing Events (Organizer):
    - Organizer logs into the organizer panel.
    - Organizer creates, updates, or cancels events.
    - System updates the database with the changes.

### Identify Microservices

- Database service ✅
    - Store data as a single source of truth
- Events service ✅
    - List all events
    - List all events based on profile’s preferences (e.g. language)
    - List my events
    - Search / Filter events by its data
    - Update events
    - Delete events
- Booking service ✅
    - Register for event
    - Cancel registration
    - Queue enrollment
    - Queue confirmation
    - QR code generation
- Notification service ✅
    - Send emails/SMS with specified content and pre-defined templates
- Queue service
    - Get queue length
    - Enqueue
    - Dequeue
    - Triggers notification for next one
- Auth service 
    - Authentication
    - Authorization

### Project Architecture Overview
#### More details in [architecture.md](.docs/architecture.md)

```mermaid
graph TB
    subgraph Layer0["Layer 0: Foundation"]
        DatabaseService[DatabaseService]
        AspNet_Common[AspNet.Common]
        AWSService[AWSService]
        Ems_Common[Ems.Common]
    end

    subgraph Layer1["Layer 1: Infrastructure Services"]
        NotificationService_Common[NotificationService.Common]
        NotificationService_Data[NotificationService.Data]
        EventService_Data[EventService.Data]
        BookingService_Data[BookingService.Data]
    end

    subgraph Layer2["Layer 2: API Services"]
        EventService_Api[EventService.Api]
        BookingService_Api[BookingService.Api]
        NotificationService_Api[NotificationService.Api]
    end

    NotificationService_Common --> AWSService
    NotificationService_Common --> Ems_Common
    NotificationService_Data --> DatabaseService
    EventService_Data --> DatabaseService
    BookingService_Data --> DatabaseService

    EventService_Api --> AspNet_Common
    EventService_Api --> Ems_Common
    EventService_Api --> DatabaseService
    EventService_Api --> EventService_Data

    BookingService_Api --> AspNet_Common
    BookingService_Api --> Ems_Common
    BookingService_Api --> DatabaseService
    BookingService_Api --> AWSService
    BookingService_Api --> NotificationService_Common
    BookingService_Api --> EventService_Data
    BookingService_Api --> BookingService_Data

    NotificationService_Api --> AspNet_Common
    NotificationService_Api --> Ems_Common
    NotificationService_Api --> DatabaseService
    NotificationService_Api --> NotificationService_Common
    NotificationService_Api --> NotificationService_Data

    style DatabaseService fill:#0052a3
    style AspNet_Common fill:#b8860b
    style AWSService fill:#ff8c00
    style NotificationService_Common fill:#e65100
    style NotificationService_Data fill:#e65100
    style NotificationService_Api fill:#e65100
    style Ems_Common fill:#b8860b
    style EventService_Data fill:#1b5e20
    style EventService_Api fill:#1b5e20
    style BookingService_Data fill:#6a1b9a
    style BookingService_Api fill:#6a1b9a

    linkStyle 0 stroke:#e65100,stroke-width:2px
    linkStyle 1 stroke:#e65100,stroke-width:2px
    linkStyle 2 stroke:#e65100,stroke-width:2px
    linkStyle 3 stroke:#1b5e20,stroke-width:2px
    linkStyle 4 stroke:#6a1b9a,stroke-width:2px
    linkStyle 5 stroke:#1b5e20,stroke-width:2px
    linkStyle 6 stroke:#1b5e20,stroke-width:2px
    linkStyle 7 stroke:#1b5e20,stroke-width:2px
    linkStyle 8 stroke:#1b5e20,stroke-width:2px
    linkStyle 9 stroke:#6a1b9a,stroke-width:2px
    linkStyle 10 stroke:#6a1b9a,stroke-width:2px
    linkStyle 11 stroke:#6a1b9a,stroke-width:2px
    linkStyle 12 stroke:#6a1b9a,stroke-width:2px
    linkStyle 13 stroke:#6a1b9a,stroke-width:2px
    linkStyle 14 stroke:#6a1b9a,stroke-width:2px
    linkStyle 15 stroke:#e65100,stroke-width:2px
    linkStyle 16 stroke:#e65100,stroke-width:2px
    linkStyle 17 stroke:#e65100,stroke-width:2px
    linkStyle 18 stroke:#e65100,stroke-width:2px
    linkStyle 19 stroke:#e65100,stroke-width:2px
```
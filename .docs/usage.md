## API Endpoints

### Event Service

#### Base URL
All endpoints are prefixed with `/v{version}/events` where version defaults to `1.0`.

Base path: `/v1/events`

#### Event Endpoints

#### Search Events
**GET** `/v1/events/search`

Search for events with optional query parameter.

**Query Parameters:**
- `query` (string, optional): Search query. Maximum 500 characters. If null or empty, returns all events.

**Response Codes:**
- `200 OK`: Success - Returns list of events
- `400 Bad Request`: Query exceeds 500 characters
- `500 Internal Server Error`: Server error

**Example Request:**
```
GET /v1/events/search?query=conference
```

**Example Response:**
```json
[
  {
    "id": "507f1f77bcf86cd799439011",
    "title": "Tech Conference 2024",
    "hostedBy": "Tech Corp",
    "isPublic": true,
    "details": "Annual technology conference",
    "timeStart": "2024-06-01T09:00:00Z",
    "timeEnd": "2024-06-01T17:00:00Z",
    "createdAt": "2024-01-01T10:00:00Z",
    "updatedAt": null
  }
]
```

#### Get Event by ID
**GET** `/v1/events/{id}`

Retrieve a specific event by its ID.

**Path Parameters:**
- `id` (string, required): Event ID (MongoDB ObjectId format)

**Response Codes:**
- `200 OK`: Success - Returns event
- `400 Bad Request`: Invalid ID format or ID is null/empty
- `404 Not Found`: Event not found
- `500 Internal Server Error`: Server error

**Example Request:**
```
GET /v1/events/507f1f77bcf86cd799439011
```

**Example Response:**
```json
{
  "id": "507f1f77bcf86cd799439011",
  "title": "Tech Conference 2024",
  "hostedBy": "Tech Corp",
  "isPublic": true,
  "details": "Annual technology conference",
  "timeStart": "2024-06-01T09:00:00Z",
  "timeEnd": "2024-06-01T17:00:00Z",
  "createdAt": "2024-01-01T10:00:00Z",
  "updatedAt": null
}
```

#### Create Event
**POST** `/v1/events`

Create a new event.

**Request Body:**
```json
{
  "title": "string (required, 3-200 characters)",
  "hostedBy": "string (required, 1-100 characters)",
  "isPublic": boolean,
  "details": "string (optional, max 2000 characters)",
  "timeStart": "DateTime (required, ISO 8601 format)",
  "timeEnd": "DateTime (required, ISO 8601 format)"
}
```

**Validation Rules:**
- `timeStart` must be before `timeEnd`
- `title`: Required, 3-200 characters
- `hostedBy`: Required, 1-100 characters
- `details`: Optional, max 2000 characters

**Response Codes:**
- `201 Created`: Success - Returns created event with location header
- `400 Bad Request`: Invalid request data or validation errors
- `500 Internal Server Error`: Server error

**Example Request:**
```
POST /v1/events
Content-Type: application/json

{
  "title": "Tech Conference 2024",
  "hostedBy": "Tech Corp",
  "isPublic": true,
  "details": "Annual technology conference",
  "timeStart": "2024-06-01T09:00:00Z",
  "timeEnd": "2024-06-01T17:00:00Z"
}
```

**Example Response:**
```json
{
  "id": "507f1f77bcf86cd799439011",
  "title": "Tech Conference 2024",
  "hostedBy": "Tech Corp",
  "isPublic": true,
  "details": "Annual technology conference",
  "timeStart": "2024-06-01T09:00:00Z",
  "timeEnd": "2024-06-01T17:00:00Z",
  "createdAt": "2024-01-01T10:00:00Z",
  "updatedAt": null
}
```

#### Update Event
**PATCH** `/v1/events/{id}`

Update an existing event. All fields are optional for partial updates.

**Path Parameters:**
- `id` (string, required): Event ID (MongoDB ObjectId format)

**Request Body:**
```json
{
  "title": "string (optional, 3-200 characters)",
  "hostedBy": "string (optional, 1-100 characters)",
  "isPublic": boolean (optional),
  "details": "string (optional, max 2000 characters)",
  "timeStart": "DateTime (optional, ISO 8601 format)",
  "timeEnd": "DateTime (optional, ISO 8601 format)"
}
```

**Validation Rules:**
- If both `timeStart` and `timeEnd` are provided, `timeStart` must be before `timeEnd`
- At least one field must be provided

**Response Codes:**
- `200 OK`: Success - Returns updated event
- `400 Bad Request`: Invalid request data, validation errors, or no valid fields to update
- `404 Not Found`: Event not found
- `500 Internal Server Error`: Server error

**Example Request:**
```
PATCH /v1/events/507f1f77bcf86cd799439011
Content-Type: application/json

{
  "title": "Updated Tech Conference 2024",
  "isPublic": false
}
```

**Example Response:**
```json
{
  "id": "507f1f77bcf86cd799439011",
  "title": "Updated Tech Conference 2024",
  "hostedBy": "Tech Corp",
  "isPublic": false,
  "details": "Annual technology conference",
  "timeStart": "2024-06-01T09:00:00Z",
  "timeEnd": "2024-06-01T17:00:00Z",
  "createdAt": "2024-01-01T10:00:00Z",
  "updatedAt": "2024-01-02T12:00:00Z"
}
```

#### Delete Event
**DELETE** `/v1/events/{id}`

Delete an event by its ID.

**Path Parameters:**
- `id` (string, required): Event ID (MongoDB ObjectId format)

**Response Codes:**
- `204 No Content`: Success - Event deleted
- `400 Bad Request`: Invalid ID format or ID is null/empty
- `404 Not Found`: Event not found
- `500 Internal Server Error`: Server error

**Example Request:**
```
DELETE /v1/events/507f1f77bcf86cd799439011
```

#### OpenAPI Specification (Development Only)
**GET** `/openapi/v1.json`

Retrieve the OpenAPI/Swagger specification for the API. Only available in development environment.

### Booking Service

#### Base URL
All endpoints are prefixed with `/v{version}/bookings` where version defaults to `1.0`.

Base path: `/v1/bookings`

#### Booking Endpoints

#### Get Booking by ID
**GET** `/v1/bookings/{id}`

Retrieve a specific booking by its ID.

**Path Parameters:**
- `id` (string, required): Booking ID (MongoDB ObjectId format)

**Response Codes:**
- `200 OK`: Success - Returns booking
- `400 Bad Request`: Invalid ID format or ID is null/empty
- `404 Not Found`: Booking not found
- `500 Internal Server Error`: Server error

**Example Request:**
```
GET /v1/bookings/507f1f77bcf86cd799439011
```

**Example Response:**
```json
{
  "id": "507f1f77bcf86cd799439011",
  "createdAt": "2024-01-01T10:00:00Z",
  "updatedAt": null
}
```

#### Create Booking
**POST** `/v1/bookings`

Create a new booking.

**Request Body:**
```json
{}
```

**Response Codes:**
- `201 Created`: Success - Returns created booking with location header
- `400 Bad Request`: Invalid request data or validation errors
- `500 Internal Server Error`: Server error

**Example Request:**
```
POST /v1/bookings
Content-Type: application/json

{}
```

**Example Response:**
```json
{
  "id": "507f1f77bcf86cd799439011",
  "createdAt": "2024-01-01T10:00:00Z",
  "updatedAt": null
}
```

#### Update Booking
**PATCH** `/v1/bookings/{id}`

Update an existing booking.

**Path Parameters:**
- `id` (string, required): Booking ID (MongoDB ObjectId format)

**Request Body:**
```json
{}
```

**Response Codes:**
- `200 OK`: Success - Returns updated booking
- `400 Bad Request`: Invalid request data, validation errors, or invalid ID format
- `404 Not Found`: Booking not found
- `500 Internal Server Error`: Server error

**Example Request:**
```
PATCH /v1/bookings/507f1f77bcf86cd799439011
Content-Type: application/json

{}
```

**Example Response:**
```json
{
  "id": "507f1f77bcf86cd799439011",
  "createdAt": "2024-01-01T10:00:00Z",
  "updatedAt": "2024-01-02T12:00:00Z"
}
```

#### Delete Booking
**DELETE** `/v1/bookings/{id}`

Delete a booking by its ID.

**Path Parameters:**
- `id` (string, required): Booking ID (MongoDB ObjectId format)

**Response Codes:**
- `204 No Content`: Success - Booking deleted
- `400 Bad Request`: Invalid ID format or ID is null/empty
- `404 Not Found`: Booking not found
- `500 Internal Server Error`: Server error

**Example Request:**
```
DELETE /v1/bookings/507f1f77bcf86cd799439011
```

# API Endpoints

## Event Service

**Service URL:** `http://localhost:5000`

#### Base URL
All endpoints are prefixed with `/v{version}/events` where version defaults to `1.0`.

Base path: `/v1/events`

#### Event Endpoints

#### Search Events
**GET** `/v1/events/search`

Search for events with optional query parameter. The query supports comma-separated keywords for multiple search terms. Searches are performed case-insensitively in the `title` and `details` fields.

**Query Parameters:**
- `query` (string, optional): Search query. Maximum 500 characters. If null or empty, returns all events. Supports comma-separated keywords (e.g., `conference,tech`).

**Response Codes:**
- `200 OK`: Success - Returns list of events
- `400 Bad Request`: Query exceeds 500 characters
- `500 Internal Server Error`: Server error

**Example Request:**
```
GET /v1/events/search?query=conference
```

**Example Request with Multiple Keywords:**
```
GET /v1/events/search?query=conference,tech,2024
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
    "available": 50,
    "timeStart": "2024-06-01T09:00:00Z",
    "timeEnd": "2024-06-01T17:00:00Z",
    "createdAt": "2024-01-01T10:00:00Z",
    "updatedAt": null
  }
]
```

**Note:** The search splits the query by commas and searches for any of the keywords in the event title or details fields. If no query is provided, all events are returned.

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
  "available": 50,
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
  "isPublic": boolean (optional, defaults to false),
  "details": "string (optional, max 2000 characters)",
  "available": "integer (required, non-negative)",
  "timeStart": "DateTime (required, ISO 8601 format)",
  "timeEnd": "DateTime (required, ISO 8601 format)"
}
```

**Validation Rules:**
- `timeStart` must be before `timeEnd`
- `title`: Required, 3-200 characters
- `hostedBy`: Required, 1-100 characters
- `isPublic`: Optional, defaults to `false`
- `details`: Optional, max 2000 characters
- `available`: Required, must be a non-negative integer (represents number of available seats)

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
  "available": 50,
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
  "available": 50,
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
  "available": "integer (optional, non-negative)",
  "timeStart": "DateTime (optional, ISO 8601 format)",
  "timeEnd": "DateTime (optional, ISO 8601 format)"
}
```

**Validation Rules:**
- If both `timeStart` and `timeEnd` are provided, `timeStart` must be before `timeEnd`
- At least one field must be provided
- All fields are optional, but at least one must be provided for the update to be valid
- `available`: If provided, must be a non-negative integer

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
  "available": 50,
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
- `500 Internal Server Error`: Server error (if deletion fails)

**Example Request:**
```
DELETE /v1/events/507f1f77bcf86cd799439011
```

#### OpenAPI Specification (Development Only)
**GET** `/openapi/v1.json`

Retrieve the OpenAPI/Swagger specification for the API. Only available in development environment.

## Booking Service

**Service URL:** `http://localhost:5001`

#### Base URL
All endpoints are prefixed with `/v{version}/bookings` where version defaults to `1.0`.

Base path: `/v1/bookings`

#### Booking Endpoints

#### Get Booking by ID
**GET** `/v1/bookings/{id}`

Retrieve a specific booking by its ID. Includes QR code data if available.

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
  "eventId": "507f1f77bcf86cd799439012",
  "status": "registered",
  "name": "John Doe",
  "email": "john.doe@example.com",
  "phone": "1234567890",
  "createdAt": "2024-01-01T10:00:00Z",
  "updatedAt": null,
  "qrCodeData": null
}
```

**Note:** `qrCodeData` is a byte array (base64-encoded in JSON) containing the QR code image data. It will be `null` if no QR code has been generated for the booking yet. QR codes are automatically generated for bookings with `registered` status.

#### Create Booking
**POST** `/v1/bookings`

Create a new booking. If status is `registered`, a QR code generation task will be queued automatically.

**Request Body:**
```json
{
  "eventId": "string (required, MongoDB ObjectId format)",
  "status": "string (required, 'registered' or 'queue_enrolled')",
  "name": "string (required, 1-100 characters)",
  "email": "string (required, valid email address)",
  "phone": "string (required, max 20 characters)"
}
```

**Validation Rules:**
- `eventId`: Required, must be a valid MongoDB ObjectId format and must reference an existing event
- `status`: Required, must be either `registered` or `queue_enrolled`
- `name`: Required, 1-100 characters
- `email`: Required, must be a valid email address
- `phone`: Required, maximum 20 characters

**Response Codes:**
- `201 Created`: Success - Returns created booking with location header
- `400 Bad Request`: Invalid request data or validation errors (including invalid eventId)
- `404 Not Found`: Referenced event not found
- `500 Internal Server Error`: Server error

**Example Request:**
```
POST /v1/bookings
Content-Type: application/json

{
  "eventId": "507f1f77bcf86cd799439012",
  "status": "registered",
  "name": "John Doe",
  "email": "john.doe@example.com",
  "phone": "1234567890"
}
```

**Example Response:**
```json
{
  "id": "507f1f77bcf86cd799439011",
  "eventId": "507f1f77bcf86cd799439012",
  "status": "registered",
  "name": "John Doe",
  "email": "john.doe@example.com",
  "phone": "1234567890",
  "createdAt": "2024-01-01T10:00:00Z",
  "updatedAt": null,
  "qrCodeData": null
}
```

**Note:** 
- When a booking is created with `queue_enrolled` status but the event has available seats (`available > 0`), the booking is automatically promoted to `registered` status.
- When a booking is created with `registered` status, a QR code generation task is automatically queued and notification tasks are enqueued.
- When a booking is created with `queue_enrolled` status, notification tasks are enqueued.
- The QR code will be available in subsequent GET requests once generation is complete.

#### Update Booking
**PATCH** `/v1/bookings/{id}`

Update an existing booking. All fields are optional for partial updates.

**Path Parameters:**
- `id` (string, required): Booking ID (MongoDB ObjectId format)

**Request Body:**
```json
{
  "name": "string (optional, 1-100 characters)",
  "email": "string (optional, valid email address)",
  "phone": "string (optional, max 20 characters)"
}
```

**Validation Rules:**
- At least one field must be provided
- `status` field is not allowed in update requests (use `Confirm` or `Cancel` endpoints to change status)
- `name`: If provided, 1-100 characters
- `email`: If provided, must be a valid email address
- `phone`: If provided, maximum 20 characters

**Response Codes:**
- `200 OK`: Success - Returns updated booking
- `400 Bad Request`: Invalid request data, validation errors, invalid ID format, or no valid fields to update
- `404 Not Found`: Booking not found
- `500 Internal Server Error`: Server error

**Note:** When a booking is updated, notification tasks are automatically enqueued.

**Example Request:**
```
PATCH /v1/bookings/507f1f77bcf86cd799439011
Content-Type: application/json

{
  "name": "Jane Doe",
  "email": "jane.doe@example.com"
}
```

**Example Response:**
```json
{
  "id": "507f1f77bcf86cd799439011",
  "eventId": "507f1f77bcf86cd799439012",
  "status": "registered",
  "name": "Jane Doe",
  "email": "jane.doe@example.com",
  "phone": "1234567890",
  "createdAt": "2024-01-01T10:00:00Z",
  "updatedAt": "2024-01-02T12:00:00Z",
  "qrCodeData": null
}
```

#### Confirm Booking
**POST** `/v1/bookings/{id}/confirm`

Confirm a booking that is in queue. Only bookings with status `queue_pending` can be confirmed. Upon confirmation, the booking status changes to `registered` and a QR code generation task is automatically queued.

**Path Parameters:**
- `id` (string, required): Booking ID (MongoDB ObjectId format)

**Response Codes:**
- `200 OK`: Success - Returns confirmed booking
- `400 Bad Request`: Invalid ID format, ID is null/empty, or booking status is not `queue_pending`
- `404 Not Found`: Booking not found
- `500 Internal Server Error`: Server error

**Note:** When a booking is confirmed, its status changes from `queue_pending` to `registered`, a QR code generation task is automatically queued, and notification tasks are enqueued. The event's available seat count is atomically decremented.

**Example Request:**
```
POST /v1/bookings/507f1f77bcf86cd799439011/confirm
```

**Example Response:**
```json
{
  "id": "507f1f77bcf86cd799439011",
  "eventId": "507f1f77bcf86cd799439012",
  "status": "registered",
  "name": "John Doe",
  "email": "john.doe@example.com",
  "phone": "1234567890",
  "createdAt": "2024-01-01T10:00:00Z",
  "updatedAt": "2024-01-02T12:00:00Z",
  "qrCodeData": null
}
```


#### Cancel Booking
**POST** `/v1/bookings/{id}/cancel`

Cancel an existing booking. Only bookings with status `registered`, `queue_enrolled`, or `queue_pending` can be canceled.

**Path Parameters:**
- `id` (string, required): Booking ID (MongoDB ObjectId format)

**Response Codes:**
- `200 OK`: Success - Returns canceled booking
- `400 Bad Request`: Invalid ID format, ID is null/empty, booking is already canceled, or booking status cannot be canceled
- `404 Not Found`: Booking not found
- `500 Internal Server Error`: Server error

**Note:** 
- When a booking is canceled, notification tasks are automatically enqueued.
- Only bookings with status `registered`, `queue_enrolled`, or `queue_pending` can be canceled.
- If a `registered` booking is canceled and there are bookings in the queue, the first booking in the queue is automatically promoted to `queue_pending` status and a notification is sent.
- If a `registered` booking is canceled and there are no bookings in the queue, the event's available seat count is incremented.

**Example Request:**
```
POST /v1/bookings/507f1f77bcf86cd799439011/cancel
```

**Example Response:**
```json
{
  "id": "507f1f77bcf86cd799439011",
  "eventId": "507f1f77bcf86cd799439012",
  "status": "canceled",
  "name": "John Doe",
  "email": "john.doe@example.com",
  "phone": "1234567890",
  "createdAt": "2024-01-01T10:00:00Z",
  "updatedAt": "2024-01-02T12:00:00Z",
  "qrCodeData": null
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
- `500 Internal Server Error`: Server error (if deletion fails)

**Example Request:**
```
DELETE /v1/bookings/507f1f77bcf86cd799439011
```

#### Booking Status Values
The following status values are supported:
- `registered`: Booking is confirmed and registered. QR code generation is automatically triggered when status is set to this value.
- `canceled`: Booking has been canceled.
- `queue_enrolled`: Booking is enrolled in a waiting queue. This status can be set when creating a booking.
- `queue_pending`: Booking is pending in the queue. This status is typically set by the system and can be confirmed to change to `registered`.

## Other Services

### LocalStack SES - Email Retrieval

**Service URL:** `http://localhost:4566`

LocalStack provides an endpoint to retrieve emails sent through the SES service. This is useful for development and testing to verify that emails are being sent correctly.

**Retrieve Sent Emails:**
```
GET http://localhost:4566/_aws/ses
```

**Example Request:**
```bash
curl http://localhost:4566/_aws/ses
```

**Example Response:**
```json
{
  "messages": [
    {
      "Id": "000001378603177f-7a5433e7-8edb-42ae-af2f-801dee0e9e8c-000000",
      "Timestamp": "2024-01-01T10:00:00Z",
      "Source": "noreply@ems.local",
      "Destination": {
        "ToAddresses": ["user@example.com"]
      },
      "Subject": "Booking Confirmation",
      "Body": {
        "Text": "Your booking has been confirmed...",
        "Html": "<html>Your booking has been confirmed...</html>"
      }
    }
  ]
}
```

**Note:** This endpoint is only available when LocalStack is running (via `docker compose up -d`). Emails sent through the BookingService notification system will appear here.

### OpenAPI Documentation

Both services provide OpenAPI/Swagger specifications in development mode.

**EventService OpenAPI:**
```
GET http://localhost:5000/openapi/v1.json
```

**BookingService OpenAPI:**
```
GET http://localhost:5001/openapi/v1.json
```

**Example Request:**
```bash
curl http://localhost:5000/openapi/v1.json
curl http://localhost:5001/openapi/v1.json
```

**Note:** OpenAPI endpoints are only available when running in Development environment. The specifications can be used with Swagger UI or other OpenAPI-compatible tools for API exploration and testing.

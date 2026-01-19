## Test cases

### BookingService.Api.Tests

#### Controllers/V1/BookingControllerTests.cs
- `GetById_WithValidId_ShouldReturnOk` - Returns OkObjectResult with booking when valid id is provided
- `GetById_WithNullId_ShouldReturnBadRequest` - Returns BadRequestObjectResult when id is null
- `GetById_WithEmptyId_ShouldReturnBadRequest` - Returns BadRequestObjectResult when id is empty
- `GetById_WithNonExistentId_ShouldReturnNotFound` - Returns NotFoundObjectResult when booking doesn't exist
- `GetById_WithInvalidFormatId_ShouldReturnBadRequest` - Returns BadRequestObjectResult when id format is invalid
- `GetById_WithException_ShouldReturn500` - Returns 500 status code when exception occurs
- `Create_WithValidDto_ShouldReturnCreatedAtAction` - Returns CreatedAtActionResult when valid dto is provided
- `Create_WithRegisteredStatus_ShouldReturnCreatedAtAction` - Returns CreatedAtActionResult when booking is created with Registered status
- `Create_WithQueueEnrolledStatus_ShouldReturnCreatedAtAction` - Returns CreatedAtActionResult when booking is created with QueueEnrolled status
- `Create_WithInvalidStatus_ShouldReturnBadRequest` - Returns BadRequestObjectResult when status is invalid
- `Create_WithInvalidModelState_ShouldReturnBadRequest` - Returns BadRequestObjectResult when model state is invalid
- `Create_WithArgumentException_ShouldReturnBadRequest` - Returns BadRequestObjectResult when ArgumentException is thrown
- `Create_WithException_ShouldReturn500` - Returns 500 status code when exception occurs
- `Update_WithValidDto_ShouldReturnOk` - Returns OkObjectResult when valid dto is provided
- `Update_WithNullId_ShouldReturnBadRequest` - Returns BadRequestObjectResult when id is null
- `Update_WithInvalidModelState_ShouldReturnBadRequest` - Returns BadRequestObjectResult when model state is invalid
- `Update_WithNonExistentId_ShouldReturnNotFound` - Returns NotFoundObjectResult when booking doesn't exist
- `Update_WithInvalidFormatId_ShouldReturnBadRequest` - Returns BadRequestObjectResult when id format is invalid
- `Update_WithArgumentException_ShouldReturnBadRequest` - Returns BadRequestObjectResult when ArgumentException is thrown
- `Update_WithException_ShouldReturn500` - Returns 500 status code when exception occurs
- `Cancel_WithValidId_ShouldReturnOk` - Returns OkObjectResult when booking is successfully canceled
- `Cancel_WithNullId_ShouldReturnBadRequest` - Returns BadRequestObjectResult when id is null
- `Cancel_WithNonExistentId_ShouldReturnNotFound` - Returns NotFoundObjectResult when booking doesn't exist
- `Cancel_WithAlreadyCanceledBooking_ShouldReturnBadRequest` - Returns BadRequestObjectResult when booking is already canceled
- `Cancel_WithQueueEnrolledStatus_ShouldReturnOk` - Returns OkObjectResult when QueueEnrolled booking is canceled
- `Cancel_WithQueuePendingStatus_ShouldReturnOk` - Returns OkObjectResult when QueuePending booking is canceled
- `Cancel_WithInvalidFormatId_ShouldReturnBadRequest` - Returns BadRequestObjectResult when id format is invalid
- `Cancel_WithException_ShouldReturn500` - Returns 500 status code when exception occurs
- `Confirm_WithValidId_ShouldReturnOk` - Returns OkObjectResult when booking is successfully confirmed
- `Confirm_WithNullId_ShouldReturnBadRequest` - Returns BadRequestObjectResult when id is null
- `Confirm_WithNonExistentId_ShouldReturnNotFound` - Returns NotFoundObjectResult when booking doesn't exist
- `Confirm_WithNonQueuePendingStatus_ShouldReturnBadRequest` - Returns BadRequestObjectResult when booking status is not queue_pending
- `Confirm_WithInvalidFormatId_ShouldReturnBadRequest` - Returns BadRequestObjectResult when id format is invalid
- `Confirm_WithException_ShouldReturn500` - Returns 500 status code when exception occurs
- `Delete_WithValidId_ShouldReturnNoContent` - Returns NoContentResult when valid id is provided
- `Delete_WithNullId_ShouldReturnBadRequest` - Returns BadRequestObjectResult when id is null
- `Delete_WithNonExistentId_ShouldReturnNotFound` - Returns NotFoundObjectResult when booking doesn't exist
- `Delete_WithInvalidFormatId_ShouldReturnBadRequest` - Returns BadRequestObjectResult when id format is invalid
- `Delete_WithException_ShouldReturn500` - Returns 500 status code when exception occurs

#### Controllers/V1/BookingControllerQrCodeTests.cs
- `GetById_WithExistingQrCode_ShouldReturnOkWithQrCodeData` - Returns OkObjectResult with booking including QR code data when QR code exists
- `GetById_WithNoQrCode_ShouldReturnOkWithNullQrCodeData` - Returns OkObjectResult with booking having null QR code data when no QR code exists

#### Controllers/V1/BookingControllerNotificationTests.cs
- `Create_WithRegisteredStatus_ShouldTriggerNotification` - Triggers email and phone notifications when booking is created with Registered status
- `Create_WithQueueEnrolledStatus_ShouldTriggerNotification` - Triggers email and phone notifications when booking is created with QueueEnrolled status
- `Update_WithValidDto_ShouldTriggerNotification` - Triggers email and phone notifications when booking is updated
- `Confirm_WithValidId_ShouldTriggerNotification` - Triggers email and phone notifications when booking is confirmed
- `Cancel_WithValidId_ShouldTriggerNotification` - Triggers email and phone notifications when booking is canceled
- `Cancel_WithQueueEnrolledStatus_ShouldTriggerNotification` - Triggers email and phone notifications when QueueEnrolled booking is canceled
- `Cancel_WithQueuePendingStatus_ShouldTriggerNotification` - Triggers email and phone notifications when QueuePending booking is canceled

#### Models/CreateBookingDtoTests.cs
- `Validate_WithValidStatus_ShouldReturnNoValidationErrors` - Returns no validation errors when dto is valid
- `Validate_WithValidStatusValues_ShouldReturnNoValidationErrors` - Returns no validation errors for Registered and QueueEnrolled statuses
- `Validate_WithInvalidStatus_ShouldReturnValidationError` - Returns validation error when status is invalid
- `Validate_WithNullStatus_ShouldReturnValidationError` - Returns validation error when Status is null
- `Validate_WithEmptyStatus_ShouldReturnValidationError` - Returns validation error when Status is empty
- `Validate_WithCanceledStatus_ShouldReturnValidationError` - Returns validation error when Canceled status is provided
- `Validate_WithQueuePendingStatus_ShouldReturnValidationError` - Returns validation error when QueuePending status is provided

#### Models/UpdateBookingDtoTests.cs
- `Validate_WithValidStatus_ShouldReturnNoValidationErrors` - Returns no validation errors when dto is valid
- `Validate_WithValidStatusValues_ShouldReturnNoValidationErrors` - Returns no validation errors for all valid statuses (Registered, Canceled, QueueEnrolled, QueuePending)
- `Validate_WithNullStatus_ShouldReturnNoValidationErrors` - Returns no validation errors when Status is null
- `Validate_WithAllNullFields_ShouldReturnNoValidationErrors` - Returns no validation errors when all fields are null
- `Validate_WithInvalidStatus_ShouldReturnValidationError` - Returns validation error when status is invalid
- `Validate_WithEmptyStatus_ShouldReturnValidationError` - Returns validation error when Status is empty

#### Models/BookingDtoTests.cs
- `Constructor_WithBooking_ShouldMapAllProperties` - Maps all properties correctly when valid booking is provided
- `Constructor_WithBookingWithNullUpdatedAt_ShouldMapNull` - Sets UpdatedAt to null when booking has null UpdatedAt
- `Constructor_WithDifferentStatuses_ShouldMapCorrectly` - Maps status correctly for all booking statuses

#### Services/HandleBookingServiceTests.cs
- `GetById_WithValidId_ShouldReturnBookingDto` - Returns BookingDto when valid id is provided
- `GetById_WithNonExistentId_ShouldThrowNotFoundException` - Throws NotFoundException when booking doesn't exist
- `GetById_WithInvalidFormatId_ShouldThrowFormatException` - Throws FormatException when id format is invalid
- `Create_WithRegisteredStatus_ShouldCreateSuccessfully` - Creates and returns booking when Registered status is provided, verifies QR code task is queued
- `Create_WithQueueEnrolledStatus_ShouldCreateSuccessfully` - Creates and returns booking when QueueEnrolled status is provided, verifies QR code task is NOT queued
- `Update_WithValidDto_ShouldUpdateAndReturnBooking` - Updates and returns booking when valid dto is provided
- `Update_WithNullStatusButOtherFields_ShouldUpdateFields` - Updates fields when Status is null but other fields are provided
- `Update_WithNoValidFields_ShouldThrowArgumentException` - Throws ArgumentException when no valid fields are provided
- `Update_WithNonExistentId_ShouldThrowNotFoundException` - Throws NotFoundException when booking doesn't exist
- `Update_WithInvalidFormatId_ShouldThrowFormatException` - Throws FormatException when id format is invalid
- `Delete_WithValidId_ShouldDeleteAndReturnTrue` - Deletes and returns true when valid id is provided
- `Delete_WithNonExistentId_ShouldReturnFalse` - Returns false when booking doesn't exist
- `Delete_WithInvalidFormatId_ShouldThrowFormatException` - Throws FormatException when id format is invalid
- `Cancel_WithValidId_ShouldCancelAndReturnBooking` - Cancels and returns booking when valid id is provided
- `Cancel_WithAlreadyCanceledBooking_ShouldThrowInvalidOperationException` - Throws InvalidOperationException when booking is already canceled
- `Cancel_WithQueueEnrolledStatus_ShouldCancelSuccessfully` - Cancels booking successfully when status is QueueEnrolled
- `Cancel_WithQueuePendingStatus_ShouldCancelSuccessfully` - Cancels booking successfully when status is QueuePending
- `Cancel_WithNonExistentId_ShouldThrowNotFoundException` - Throws NotFoundException when booking doesn't exist
- `Cancel_WithInvalidFormatId_ShouldThrowFormatException` - Throws FormatException when id format is invalid
- `Confirm_WithValidId_ShouldConfirmAndReturnBooking` - Confirms and returns booking when valid id is provided
- `Confirm_WithNonQueuePendingStatus_ShouldThrowInvalidOperationException` - Throws InvalidOperationException when booking status is not queue_pending
- `Confirm_WithNonExistentId_ShouldThrowNotFoundException` - Throws NotFoundException when booking doesn't exist
- `Confirm_WithInvalidFormatId_ShouldThrowFormatException` - Throws FormatException when id format is invalid

#### Services/HandleBookingServiceQrCodeTests.cs
- `GetById_WithExistingQrCode_ShouldReturnBookingDtoWithQrCodeData` - Returns BookingDto with QR code data when QR code exists
- `GetById_WithNoQrCode_ShouldReturnBookingDtoWithNullQrCodeData` - Returns BookingDto with null QR code data when no QR code exists

#### Services/HandleBookingServiceNotificationTests.cs
- `Create_WithRegisteredStatus_ShouldTriggerNotification` - Triggers email and phone notifications when booking is created with Registered status
- `Create_WithQueueEnrolledStatus_ShouldTriggerNotification` - Triggers email and phone notifications when booking is created with QueueEnrolled status
- `Update_WithValidDto_ShouldTriggerNotification` - Triggers email and phone notifications when booking is updated
- `Update_WithNullStatusButOtherFields_ShouldTriggerNotification` - Triggers email and phone notifications when booking is updated with null status but other fields
- `Cancel_WithValidId_ShouldTriggerNotification` - Triggers email and phone notifications when booking is canceled
- `Cancel_WithQueueEnrolledStatus_ShouldTriggerNotification` - Triggers email and phone notifications when QueueEnrolled booking is canceled
- `Cancel_WithQueuePendingStatus_ShouldTriggerNotification` - Triggers email and phone notifications when QueuePending booking is canceled
- `Confirm_WithValidId_ShouldTriggerNotification` - Triggers email and phone notifications when booking is confirmed

#### Services/HandleQrCodeService/QrCodeTaskProcessorTests.cs
- `ProcessAsync_WithValidMessage_ShouldGenerateAndSaveQrCode` - Generates and saves QR code when valid message is provided
- `ProcessAsync_ShouldGenerateValidQrCodeData` - Generates valid QR code data with appropriate length
- `ProcessAsync_WithDifferentBookingIds_ShouldGenerateDifferentQrCodes` - Generates different QR codes for different booking IDs
- `ProcessAsync_ShouldLogInformationMessages` - Logs information messages during QR code generation

### BookingService.Data.Tests

#### Models/BookingTests.cs
- `GetEntityFromDto_ShouldMapAllPropertiesCorrectly` - Maps all properties correctly from dto to entity
- `GetEntityFromDto_WithDifferentStatuses_ShouldMapCorrectly` - Maps status correctly for all booking statuses
- `GetEntityFromDto_ShouldSetCreatedAtToUtcNow` - Sets CreatedAt to UtcNow when creating entity
- `Booking_WithValidStatus_ShouldHaveCorrectStatus` - Verifies booking has correct status

#### Repositories/BookingRepositoryTests.cs
- `GetByIdAsync_WithValidId_ShouldReturnBooking` - Returns booking when valid id is provided
- `GetByIdAsync_WithNonExistentId_ShouldThrowNotFoundException` - Throws NotFoundException when booking doesn't exist
- `GetByIdAsync_WithInvalidObjectId_ShouldThrowFormatException` - Throws FormatException when id format is invalid
- `GetAllAsync_ShouldReturnAllBookings` - Returns all bookings from collection
- `CreateAsync_ShouldInsertAndReturnBooking` - Inserts and returns booking when valid booking is provided
- `UpdateAsync_WithValidId_ShouldUpdateAndReturnBooking` - Updates and returns booking when valid id is provided
- `UpdateAsync_WithNonExistentId_ShouldThrowNotFoundException` - Throws NotFoundException when booking doesn't exist
- `DeleteAsync_WithValidId_ShouldReturnTrue` - Returns true when booking is successfully deleted
- `DeleteAsync_WithNonExistentId_ShouldReturnFalse` - Returns false when booking doesn't exist

#### Repositories/QrCodeRepositoryTests.cs
- `GetByBookingIdAsync_WithExistingBookingId_ShouldReturnQrCode` - Returns QR code when booking ID exists
- `GetByBookingIdAsync_WithNonExistentBookingId_ShouldReturnNull` - Returns null when booking ID doesn't exist
- `GetByIdAsync_WithValidId_ShouldReturnQrCode` - Returns QR code when valid id is provided
- `GetByIdAsync_WithNonExistentId_ShouldThrowNotFoundException` - Throws NotFoundException when QR code doesn't exist
- `CreateAsync_ShouldInsertAndReturnQrCode` - Inserts and returns QR code when valid QR code is provided
- `DeleteAsync_WithValidId_ShouldReturnTrue` - Returns true when QR code is successfully deleted
- `DeleteAsync_WithNonExistentId_ShouldReturnFalse` - Returns false when QR code doesn't exist

### EventService.Api.Tests

#### Controllers/V1/EventControllerTests.cs
- `Search_WithValidQuery_ShouldReturnOk` - Returns OkObjectResult with events when valid query is provided
- `Search_WithNullQuery_ShouldReturnOk` - Returns OkObjectResult when query is null
- `Search_WithQueryExceeding500Characters_ShouldReturnBadRequest` - Returns BadRequestObjectResult when query exceeds 500 characters
- `Search_WithException_ShouldReturn500` - Returns 500 status code when exception occurs
- `GetById_WithValidId_ShouldReturnOk` - Returns OkObjectResult with event when valid id is provided
- `GetById_WithNullId_ShouldReturnBadRequest` - Returns BadRequestObjectResult when id is null
- `GetById_WithEmptyId_ShouldReturnBadRequest` - Returns BadRequestObjectResult when id is empty
- `GetById_WithNonExistentId_ShouldReturnNotFound` - Returns NotFoundObjectResult when event doesn't exist
- `GetById_WithInvalidFormatId_ShouldReturnBadRequest` - Returns BadRequestObjectResult when id format is invalid
- `GetById_WithException_ShouldReturn500` - Returns 500 status code when exception occurs
- `Create_WithValidDto_ShouldReturnCreatedAtAction` - Returns CreatedAtActionResult when valid dto is provided
- `Create_WithInvalidModelState_ShouldReturnBadRequest` - Returns BadRequestObjectResult when model state is invalid
- `Create_WithArgumentException_ShouldReturnBadRequest` - Returns BadRequestObjectResult when ArgumentException is thrown
- `Create_WithException_ShouldReturn500` - Returns 500 status code when exception occurs
- `Update_WithValidDto_ShouldReturnOk` - Returns OkObjectResult when valid dto is provided
- `Update_WithNullId_ShouldReturnBadRequest` - Returns BadRequestObjectResult when id is null
- `Update_WithInvalidModelState_ShouldReturnBadRequest` - Returns BadRequestObjectResult when model state is invalid
- `Update_WithNonExistentId_ShouldReturnNotFound` - Returns NotFoundObjectResult when event doesn't exist
- `Update_WithInvalidFormatId_ShouldReturnBadRequest` - Returns BadRequestObjectResult when id format is invalid
- `Update_WithArgumentException_ShouldReturnBadRequest` - Returns BadRequestObjectResult when ArgumentException is thrown
- `Update_WithException_ShouldReturn500` - Returns 500 status code when exception occurs
- `Delete_WithValidId_ShouldReturnNoContent` - Returns NoContentResult when valid id is provided
- `Delete_WithNullId_ShouldReturnBadRequest` - Returns BadRequestObjectResult when id is null
- `Delete_WithNonExistentId_ShouldReturnNotFound` - Returns NotFoundObjectResult when event doesn't exist
- `Delete_WithInvalidFormatId_ShouldReturnBadRequest` - Returns BadRequestObjectResult when id format is invalid
- `Delete_WithException_ShouldReturn500` - Returns 500 status code when exception occurs

#### Models/CreateEventDtoTests.cs
- `Validate_WithValidDto_ShouldReturnNoValidationErrors` - Returns no validation errors when dto is valid
- `Validate_WithTimeStartAfterTimeEnd_ShouldReturnValidationError` - Returns validation error when TimeStart is after TimeEnd
- `Validate_WithTimeStartEqualToTimeEnd_ShouldReturnValidationError` - Returns validation error when TimeStart equals TimeEnd
- `Validate_WithTitleTooShort_ShouldReturnValidationError` - Returns validation error when Title is too short
- `Validate_WithTitleTooLong_ShouldReturnValidationError` - Returns validation error when Title is too long
- `Validate_WithHostedByTooShort_ShouldReturnValidationError` - Returns validation error when HostedBy is too short
- `Validate_WithHostedByTooLong_ShouldReturnValidationError` - Returns validation error when HostedBy is too long
- `Validate_WithDetailsTooLong_ShouldReturnValidationError` - Returns validation error when Details exceeds maximum length
- `Validate_WithNullDetails_ShouldReturnNoValidationError` - Returns no validation errors when Details is null

#### Models/EventDtoTests.cs
- `Constructor_WithValidEvent_ShouldMapAllProperties` - Maps all properties correctly when valid event is provided
- `Constructor_WithNullUpdatedAt_ShouldSetUpdatedAtToNull` - Sets UpdatedAt to null when event has null UpdatedAt
- `Constructor_WithUpdatedAt_ShouldMapUpdatedAt` - Maps UpdatedAt correctly when event has UpdatedAt
- `Constructor_WithNullDetails_ShouldSetDetailsToNull` - Sets Details to null when event has null Details
- `Constructor_WithNullEvent_ShouldThrowNullReferenceException` - Throws NullReferenceException when event is null
- `Constructor_WithEventHavingAllProperties_ShouldMapCorrectly` - Maps all properties correctly when event has all properties

#### Models/UpdateEventDtoTests.cs
- `Validate_WithValidDto_ShouldReturnNoValidationErrors` - Returns no validation errors when dto is valid
- `Validate_WithTimeStartAfterTimeEnd_ShouldReturnValidationError` - Returns validation error when TimeStart is after TimeEnd
- `Validate_WithOnlyTimeStart_ShouldReturnNoValidationErrors` - Returns no validation errors when only TimeStart is provided
- `Validate_WithOnlyTimeEnd_ShouldReturnNoValidationErrors` - Returns no validation errors when only TimeEnd is provided
- `Validate_WithPartialUpdate_ShouldReturnNoValidationErrors` - Returns no validation errors when partial update is provided
- `Validate_WithTitleTooShort_ShouldReturnValidationError` - Returns validation error when Title is too short
- `Validate_WithTitleTooLong_ShouldReturnValidationError` - Returns validation error when Title is too long
- `Validate_WithHostedByTooShort_ShouldReturnValidationError` - Returns validation error when HostedBy is too short
- `Validate_WithHostedByTooLong_ShouldReturnValidationError` - Returns validation error when HostedBy is too long
- `Validate_WithDetailsTooLong_ShouldReturnValidationError` - Returns validation error when Details exceeds maximum length
- `Validate_WithNullTitle_ShouldReturnNoValidationError` - Returns no validation errors when Title is null

#### Services/HandleEventServiceTests.cs
- `Search_WithNullQuery_ShouldReturnAllEvents` - Returns all events when query is null
- `Search_WithEmptyQuery_ShouldReturnAllEvents` - Returns all events when query is empty
- `Search_WithSingleKeyword_ShouldFilterEvents` - Filters events when single keyword is provided
- `Search_WithMultipleKeywords_ShouldFilterEvents` - Filters events when multiple keywords are provided
- `GetById_WithValidId_ShouldReturnEventDto` - Returns EventDto when valid id is provided
- `GetById_WithNonExistentId_ShouldThrowNotFoundException` - Throws NotFoundException when event doesn't exist
- `GetById_WithInvalidFormatId_ShouldThrowFormatException` - Throws FormatException when id format is invalid
- `Create_WithValidDto_ShouldCreateAndReturnEvent` - Creates and returns event when valid dto is provided
- `Update_WithValidDto_ShouldUpdateAndReturnEvent` - Updates and returns event when valid dto is provided
- `Update_WithNoValidFields_ShouldThrowArgumentException` - Throws ArgumentException when no valid fields are provided
- `Update_WithNonExistentId_ShouldThrowNotFoundException` - Throws NotFoundException when event doesn't exist
- `Update_WithInvalidFormatId_ShouldThrowFormatException` - Throws FormatException when id format is invalid
- `Update_WithPartialFields_ShouldUpdateOnlySpecifiedFields` - Updates only specified fields when partial update is provided
- `Delete_WithValidId_ShouldDeleteAndReturnTrue` - Deletes and returns true when valid id is provided
- `Delete_WithNonExistentId_ShouldReturnFalse` - Returns false when event doesn't exist
- `Delete_WithInvalidFormatId_ShouldThrowFormatException` - Throws FormatException when id format is invalid

### EventService.Data.Tests

#### Models/EventTests.cs
- `GetEntityFromDto_ShouldMapAllPropertiesCorrectly` - Maps all properties correctly from dto to entity
- `GetEntityFromDto_ShouldHandleNullDetails` - Handles null Details correctly
- `GetEntityFromDto_ShouldSetCreatedAtToUtcNow` - Sets CreatedAt to UtcNow when creating entity

#### Repositories/EventRepositoryTests.cs
- `SearchAsync_WithNullQuery_ShouldReturnAllEvents` - Returns all events when query is null
- `SearchAsync_WithEmptyQuery_ShouldReturnAllEvents` - Returns all events when query is empty
- `SearchAsync_WithWhitespaceQuery_ShouldReturnAllEvents` - Returns all events when query contains only whitespace
- `SearchAsync_WithSingleKeyword_ShouldFilterByTitleOrDetails` - Filters events by title or details when single keyword is provided
- `SearchAsync_WithMultipleKeywords_ShouldFilterByAnyKeyword` - Filters events by any keyword when multiple keywords are provided
- `SearchAsync_WithCommaSeparatedKeywords_ShouldTrimAndFilter` - Trims and filters events when comma-separated keywords are provided
- `SearchAsync_WithEmptyKeywordsAfterSplit_ShouldReturnAllEvents` - Returns all events when query contains only commas
- `SearchAsync_WithSpecialCharacters_ShouldEscapeRegex` - Escapes special characters in search query
- `SearchAsync_ShouldBeCaseInsensitive` - Performs case-insensitive search

### DatabaseService.Tests

#### MongoDbContextTests.cs
- `GetCollection_ShouldReturnCollectionWithCorrectName` - Returns collection with correct name
- `GetCollection_ShouldReturnCollectionWithCorrectType` - Returns collection with correct type
- `GetCollection_ShouldReturnDifferentCollectionsForDifferentNames` - Returns different collections for different names

#### Repositories/RepositoryTests.cs
- `GetByIdAsync_WithValidId_ShouldReturnEntity` - Returns entity when valid id is provided
- `GetByIdAsync_WithNonExistentId_ShouldThrowNotFoundException` - Throws NotFoundException when entity doesn't exist
- `GetByIdAsync_WithInvalidObjectId_ShouldThrowFormatException` - Throws FormatException when id format is invalid
- `GetAllAsync_ShouldReturnAllEntities` - Returns all entities from collection
- `GetAllAsync_WithEmptyCollection_ShouldReturnEmptyList` - Returns empty list when collection is empty
- `CreateAsync_ShouldInsertAndReturnEntity` - Inserts and returns entity when valid entity is provided
- `UpdateAsync_WithValidId_ShouldUpdateAndReturnEntity` - Updates and returns entity when valid id is provided
- `UpdateAsync_WithNonExistentId_ShouldThrowNotFoundException` - Throws NotFoundException when entity doesn't exist
- `UpdateAsync_WithInvalidObjectId_ShouldThrowFormatException` - Throws FormatException when id format is invalid
- `DeleteAsync_WithValidId_ShouldReturnTrue` - Returns true when entity is successfully deleted
- `DeleteAsync_WithNonExistentId_ShouldReturnFalse` - Returns false when entity doesn't exist
- `DeleteAsync_WithInvalidObjectId_ShouldThrowFormatException` - Throws FormatException when id format is invalid

#### Exceptions/NotFoundExceptionTests.cs
- `NotFoundException_ShouldHaveCorrectMessage` - Has correct exception message format
- `NotFoundException_ShouldBeExceptionType` - Is assignable to Exception type
- `NotFoundException_WithDifferentCollectionName_ShouldHaveCorrectMessage` - Has correct message with different collection name

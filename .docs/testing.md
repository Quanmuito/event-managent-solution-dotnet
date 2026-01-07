## Test cases

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
- `Delete_WithException_ShouldReturn500` - Returns 500 status code when exception occurs

#### Models/Api/Event/CreateEventDtoTests.cs
- `Validate_WithValidDto_ShouldReturnNoValidationErrors` - Returns no validation errors when dto is valid
- `Validate_WithTimeStartAfterTimeEnd_ShouldReturnValidationError` - Returns validation error when TimeStart is after TimeEnd
- `Validate_WithTimeStartEqualToTimeEnd_ShouldReturnValidationError` - Returns validation error when TimeStart equals TimeEnd

#### Models/Api/Event/EventDtoTests.cs
- `Constructor_WithValidEvent_ShouldMapAllProperties` - Maps all properties correctly when valid event is provided
- `Constructor_WithNullUpdatedAt_ShouldSetUpdatedAtToNull` - Sets UpdatedAt to null when event has null UpdatedAt
- `Constructor_WithUpdatedAt_ShouldMapUpdatedAt` - Maps UpdatedAt correctly when event has UpdatedAt
- `Constructor_WithNullDetails_ShouldSetDetailsToNull` - Sets Details to null when event has null Details
- `Constructor_WithNullEvent_ShouldThrowNullReferenceException` - Throws NullReferenceException when event is null
- `Constructor_WithEventHavingAllProperties_ShouldMapCorrectly` - Maps all properties correctly when event has all properties

#### Models/Api/Event/UpdateEventDtoTests.cs
- `Validate_WithValidDto_ShouldReturnNoValidationErrors` - Returns no validation errors when dto is valid
- `Validate_WithTimeStartAfterTimeEnd_ShouldReturnValidationError` - Returns validation error when TimeStart is after TimeEnd
- `Validate_WithOnlyTimeStart_ShouldReturnNoValidationErrors` - Returns no validation errors when only TimeStart is provided
- `Validate_WithOnlyTimeEnd_ShouldReturnNoValidationErrors` - Returns no validation errors when only TimeEnd is provided
- `Validate_WithPartialUpdate_ShouldReturnNoValidationErrors` - Returns no validation errors when partial update is provided

#### Models/Domain/EventTests.cs
- `GetEntityFromDto_ShouldMapAllPropertiesCorrectly` - Maps all properties correctly from dto to entity
- `GetEntityFromDto_ShouldHandleNullDetails` - Handles null Details correctly
- `GetEntityFromDto_ShouldSetCreatedAtToUtcNow` - Sets CreatedAt to UtcNow when creating entity

#### Services/HandleEventServiceTests.cs
- `Search_WithNullQuery_ShouldReturnAllEvents` - Returns all events when query is null
- `Search_WithEmptyQuery_ShouldReturnAllEvents` - Returns all events when query is empty
- `Search_WithSingleKeyword_ShouldFilterEvents` - Filters events when single keyword is provided
- `Search_WithMultipleKeywords_ShouldFilterEvents` - Filters events when multiple keywords are provided
- `GetById_WithValidId_ShouldReturnEventDto` - Returns EventDto when valid id is provided
- `GetById_WithNonExistentId_ShouldThrowNullReferenceException` - Throws NullReferenceException when event doesn't exist
- `Create_WithValidDto_ShouldCreateAndReturnEvent` - Creates and returns event when valid dto is provided
- `Update_WithValidDto_ShouldUpdateAndReturnEvent` - Updates and returns event when valid dto is provided
- `Update_WithNoValidFields_ShouldThrowArgumentException` - Throws ArgumentException when no valid fields are provided
- `Delete_WithValidId_ShouldDeleteAndReturnDeleteResult` - Deletes and returns DeleteResult when valid id is provided
- `Delete_WithNonExistentId_ShouldReturnDeleteResultWithZeroCount` - Returns DeleteResult with zero count when event doesn't exist

### EventService.Data.Tests

#### MongoDbContextTests.cs
- `GetCollection_ShouldReturnCollectionWithCorrectName` - Returns collection with correct name
- `GetCollection_ShouldReturnCollectionWithCorrectType` - Returns collection with correct type

# UserInfoService

This repository contains a sample .NET 6 Web API project handling user information using clean architecture.

## Prerequisites

- .NET 6 SDK
- Visual Studio 2022
- Git

## Getting Started

1. Clone the repository
```
git clone https://github.com/dilani14/UserInfoService.git
```
2. Navigate to project directory
```
cd .\UserInfoService\UserInfoService.API\
```
3. Build and run the project
```
dotent run
```
4. Open your web browser and access the API at **'https://localhost:5000/api/userinfo.'**

## Testing
Unit tests are located in the 'tests' folder. The project uses xUnit for testing.

To run tests,

1. Navigate to the project directory
```
cd .\UserInfoService\UserInfoService.Core.Test\
```
2. Execute the tests
```
dotnet test
```

## Endpoints
This Web API provides the following endpoints:

- GET /api/userinfo: Get a list of user informatiom.
- POST /api/userinfo: Create a new user.
- PUT /api/userinfo/{id}: Update an existing user.
- DELETE /api/userinfo/{id}: Delete a user.

## Caching
Caching is configured using In-Memory Cache.

## Logging
Logging is configured using the built-in logging framework in .NET 6. 

## Error Handling
The project uses global exception handling to return appropriate HTTP status codes and error messages.


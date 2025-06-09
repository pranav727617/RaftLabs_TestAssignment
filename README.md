ExternalUserService - Reqres.in API Client Library
NET class library - [Reqres.in API](https://reqres.in/) to fetch and process user data. Its a robust and testable program for integrating external APIs, including features like paging, caching, configuration, error handling, and unit testing.

Feature:
1. Async API client using `HttpClient` and `IHttpClientFactory`
2. Fetch individual user by ID
3. Fetch all users with paging
4. Using options pattern. API base URL and caching expiration.
5. Logging support
6. Unit tests using xUnit and Moq
7. Use of Polly

Build the Solution:
1. Clone the repository
2. dotnet restore
3. dotnet build

To run the Test - dotnet test

Design:
1. HttpClient - To avoid socket exhaustion
2. Options Pattern - API base URL and cache expiration
3. In-Memory Caching - Reducing API calls
4. Async/Await with paging - Asynchronous calls with pagination
5. Error Handling and Logging - Logging of HTTP Failures
6. Moq and xUnit - Mock HTTP responses, covered success and failure scenerios
7. Polly - Added for transient error handling

DemoConsole App:
Location - RaftLabsAssignement/RaftLabsAssignmentDemo/ExternalUserService.Demo

Run DemoConsole App:
1. Open Solution in VS
2. Right-click ExternalUserService.Demo → Set as Startup Project.
3. Press Run

Output: User: Janet Weaver, Email: janet.weaver@reqres.in
Press any key to exit...

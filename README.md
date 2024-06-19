# Amega Senior Developer test

### 1. Demonstrating Knowledge and Skills:

While this approach might extend beyond the original requirements, I implemented it to demonstrate my proficiency in various libraries and tasks.

This implementation ensures that users can easily test and validate the functionality by simply calling the provided endpoints.

#  PriceController API

The `PriceController` provides endpoints to Fetch Available Instruments and get the latest price for one instrument. This controller interacts with the `IAlphaVantageService` to perform its operations.

## Endpoints

### 1. Get Symbols

**Endpoint**: `GET /api/v1/Price/GetAvailableSymbols`

**Description**: 
This endpoint is designed to retrieve available symbols. The service we are using does not provide a direct method to fetch available symbols. Initially, I attempted to implement a workaround by fetching symbols and checking if they returned a valid response. If they did, I would add them to a list and return them. However, due to the limitations of the free version of the service, I decided to mock the 3 symbols requested in the tests.

**Response**:
- **200 OK**: Returns the processed story details.
- **500 Internal Server Error**: If there is an issue during the insertion or processing of the story.

**Example Request**:
```http
GET /api/v1/Price/GetAvailableSymbols
```

**Example Response**:
```http
[
  "EURUSD",
  "USDJPY",
  "BTCUSD"
]
```
 

### 2. Get Price By Symbol

**Endpoint**: `GET /api/v1/Price/GetPriceBySymbol/{symbol}`


**Description**: 

This endpoint retrieves the latest price information for a given symbol.



**Parameters**: 

- symbol (string): The symbol for which you want to retrieve the price information. For example, AAPL for Apple Inc. or BTCUSD for Bitcoin to USD.


**Response**:
- **200 OK**: Returns the processed story details.
- **404 Not Found**: If the story with the specified ID is not found.
- **500 Internal Server Error**: If there is an issue during the insertion or processing of the story.



**Example Request**:
```http
GET  /api/v1/Price/GetPriceBySymbol/AAPL
```



**Example Response**:
```http
{
  "instrument": "APPL",
  "price": 1,
  "timestamp": "2024-06-19T09:52:15.3530359+02:00"
}
```

As the `AlphaVantageService` Service has limited amounts of requests as being the free version, I mocked the response only when debugging and the response is null
```cs
            if (priceResponse == null || priceResponse.TimeSeries == null)
            {
                _logger.LogWarning($"Price information for symbol {symbol} not found or empty response.");
            }

#if DEBUG
            if (priceResponse == null || priceResponse.TimeSeries == null)
            {
                _logger.LogDebug("Using debug fallback data for price response.");
                priceResponse = new AlphaVantageResponse { TimeSeries = new Dictionary<string, TimeSeriesEntry>() };
                priceResponse.TimeSeries.Add("2021-01-01 00:00:00", new TimeSeriesEntry
                {
                    Open = "0.92",
                    High = "1.12",
                    Low = "1.88",
                    Close = "1.0",
                    Volume = "1.0",
                    Timestamp = DateTime.Now
                });
            }
#endif
```

# WebSocketHandlerMiddleware

The `WebSocketHandlerMiddleware` is a middleware component for handling WebSocket connections in an ASP.NET Core application. It facilitates accepting WebSocket requests, managing WebSocket connections, and broadcasting messages to connected clients.

## Features

- Accepts and manages WebSocket connections
- Handles incoming WebSocket messages
- Broadcasts price updates to all connected clients

## Methods

### `InvokeAsync(HttpContext context)`

Accepts WebSocket requests and manages the WebSocket connections.

**Parameters:**
- `HttpContext context`: The HTTP context for the current request.

### `HandleWebSocketAsync(WebSocket webSocket)`

Handles incoming WebSocket messages and manages the WebSocket connection lifecycle.

**Parameters:**
- `WebSocket webSocket`: The WebSocket connection to handle.

### `BroadcastPriceUpdates(List<PriceUpdate> priceUpdates)`

Broadcasts price updates to all connected WebSocket clients.

**Parameters:**
- `List<PriceUpdate> priceUpdates`: The list of price updates to broadcast.
 

## Comments on Handling 1,000+ Subscribers

- **Efficient WebSocket Management**: Use a concurrent collection for managing WebSocket connections if necessary to handle high concurrency.

- **Load Balancing**: For production environments, use a load balancer to distribute WebSocket connections across multiple instances of your service.

- **Resource Optimization**: Monitor CPU, memory, and network usage to ensure the service can handle the load. Optimize data fetching intervals and reduce unnecessary operations.
## Additional Implementation Details

Even though it was not required, I have added some simple tests to showcase my skills. Here are the key additions:

- **IHttpClientWrapper**: Created an `IHttpClientWrapper` to abstract HTTP client operations and make the code more testable.
- **IPriceUpdateService Service Registration**: Registered the service twice to be able to Test it.
- **Unit Tests with NUnit**: Added unit tests using NUnit to ensure the correctness and reliability of the code.

These additions demonstrate my ability to write clean, maintainable, and testable code while adhering to best practices.

Please keep in mind they are small and simple tests, for a small and simple scenario. 

## Tech Stack:

#### C# .NET Core: 
A cross-platform framework for building web APIs and applications using C#.
#### API Documentation and Testing:
Swagger: A tool for API documentation and testing, providing a UI for visualizing and interacting with APIs.
#### Testing Framework:
NUnit: A unit testing framework for .NET applications, used to write and execute tests for the API.


## Setup Steps

### 1.Clone the Repository

Clone the repository or download the source code as a ZIP file and extract it to your desired location.

### 2.Update ApiKey for AlphaVantageService

Navigate to the appsettings.json file located in the root directory of the project.

Update the DefaultConnection string with your SQL Server connection string:

```csharp
 "AlphaVantage": {
   "ApiKey": "YOUR_API_KEY"
 },

```
 
### 3. Build and Run the Project

#### Using Visual Studio

1) Open the solution file (TestProject.sln) in Visual Studio.

2) Set the startup project to WebApi Project.

3) Press F5 or click Start Debugging to build and run the project.

#### Using Visual Studio

1) Open a terminal or command prompt.

2) Navigate to the project directory where the .csproj file is located.

3) Run the following command to build and run the project:


```bash
dotnet run
```
 

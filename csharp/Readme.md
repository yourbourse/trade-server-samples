# API Example Project

This project demonstrates how to interact with a trading server using both HTTP APIs and WebSocket communication. It includes examples of authentication, querying symbols, and sending WebSocket messages.


## Table of Contents

1. [Features](#features)
2. [Project Structure](#project-structure)
3. [Setup and Configuration](#setup-and-configuration)
4. [Usage](#usage)
    - [Authentication](#authentication)
    - [Querying Symbols](#querying-symbols)
    - [WebSocket Communication](#websocket-communication)
5. [Code Examples](#code-examples)
6. [Requirements](#requirements)
8. [Troubleshooting](#troubleshooting)
9. [Notes](#notes)

## Features

- **Authentication**:
    - Supports two methods of authentication: `Nonce` and `Timestamp`.
    - Demonstrates secure API token retrieval.
- **HTTP API Examples**:
    - Authorize a user and retrieve an API token.
    - Query available trading symbols.
- **WebSocket Examples**:
    - Connect to a WebSocket server.
    - Send and receive WebSocket messages.
    - Handle WebSocket events such as `ping`, `subscribe`, and `unsubscribe`.
- **Error Handling**:
    - Graceful handling of WebSocket disconnections and errors.
    - Logging of API responses and WebSocket messages.


## Project Structure

- **`WebSockerHelpers.cs`**:
    - Contains helper methods for sending and receiving WebSocket messages.
- **`Program.cs`**:
    - The main entry point of the application, demonstrating API and WebSocket usage.
- **`AuthenticationMethod.cs`**:
    - Defines the `AuthenticationMethod` enum for specifying authentication types.
- **`Readme.md`**:
    - Documentation for the project.

## Setup and Configuration

1. **Clone the Repository**:
   ```bash
   
   git clone https://github.com/yourbourse/trade-server-samples.git
   cd trade-server-samples
   ```

2. **Update Configuration**:
3. Open `Program.cs` and update the following variables with your credentials and server details:
   ```csharp
   const int login = 1;
   const string password = "YourPassword";
   const string tradeServerUrl = "https://yourbourse.trade:2xxxx";
   const string wsUrl = "wss://yourbourse.trade:3xxxx/ws/v1";
   ```
4. **Build the Project**:

Use your IDE (e.g., JetBrains Rider) or run the following command:

```
cd csharp/api-example
dotnet build
```

5. **Run the Application**:

Execute the project:
```
dotnet run
```

## Usage
### Authentication
 
- The application demonstrates two methods of authentication: `Nonce` and `Timestamp`.
* **Nonce**: A one-time token-based authentication.
* **Timestamp**: A time-based token authentication.
- Examples of both methods are provided in the `Program.cs` file.
  Authenticate using Nonce:
    ```csharp
    var authNonceResponse = await Authorise(login, AuthenticationMethod.Nonce);
    ```

### Querying Symbols
- The application queries available trading symbols using the `QuerySymbols` method.
- The response is logged to the console.
- Example:
  ```csharp
  await QuerySymbols(authNonceResponse.Token, AuthenticationMethod.Nonce);
  ```
  
### WebSocket Communication
- The application connects to the WebSocket server and demonstrates sending and receiving messages.
  - It handles `ping`, `subscribe`, and `unsubscribe` messages.
    - Example:
      ```csharp
      var socket = new ClientWebSocket();
      await socket.ConnectAsync(new Uri(wsUrl), CancellationToken.None);
      ```
      Send WebSocket messages:
      - Ping
        ```json
        {
          "m": "ping",
          "h": {
              "X-YB-API-Key": "YourApiKey",
              "X-YB-LOCALE": "en"
             },
          "reqId": "777816"
        }
        ```
      - Subscribe
        ```json
        {
          "m": "subscribe",
          "c": "L1",
          "p": {
          "s": "EURUSD",
            "streaming": false
          },
          "h": {
            "X-YB-API-Key": "YourApiKey",
            "X-YB-LOCALE": "en"
          },
          "reqId": "777817"
         }
        ```
      - Unsubscribe
          ```json
          {
          "m": "unsubscribe",
          "c": "L1",
          "p": {
              "s": "EURUSD",
          },
          "h": {
              "X-YB-API-Key": "YourApiKey",
              "X-YB-LOCALE": "en"
          },
          "reqId": "777818"
          }
          ```
### Code Examples

Sending WebSocket messages:
```csharp
await WebSockerHelpers.SendAsync(socket, pingMessageJson);
```
  
Receiving WebSocket messages:
```csharp
var listenTask = Task.Run(async () => await WebSockerHelpers.ListenWebSocket(socket, CancellationToken.None));
```

## Requirements

- Internet connection to access the trading server

### Troubleshooting
1. WebSocket Connection Issues:  
   - Ensure the wsUrl is correct and accessible.
   - Check for firewall or network restrictions.
2. Authentication Failures:  
   - Verify the login and password values.
   - Ensure the API server is reachable.
3. Unhandled Exceptions:  
   - Review the console logs for error messages.
   - Add additional exception handling where necessary.

## Notes

- Ensure that the API key and credentials are kept secure and not hardcoded in production environments.
- Handle WebSocket errors and disconnections gracefully in real-world applications.
- Trade Server API port starts with 2, Web Socket port starts with 3.


---

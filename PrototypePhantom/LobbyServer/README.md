This script defines a `LobbyManager` class that manages a collection of `Lobby` instances, each of which contains a collection of `Room` instances. The `LobbyManager` class provides methods for adding and removing players and spectators, getting the status of a room, voting for a map, and starting a game. The `Lobby`, `Room`, and `LobbyServer` classes provide implementations for these methods.

The `LobbyServer` class is the main entry point of the script. It sets up a UDP socket, binds it to a specific address (in this case, `localhost` and port `8000`), and listens for incoming packets. When a packet is received, the server starts a new thread to handle the request, so that it can continue listening for new requests.

The `handle_client` method is responsible for parsing the incoming request, calling the appropriate method on the `LobbyManager`, and sending a response back to the client. The request is expected to be in JSON format and have the following structure:
```json
{
    "command": "<name of method to call>",
    "args": [<list of arguments for method>]
}
```

The response has the following structure:

```json
{
    "status": "<status of operation: 'success' or 'error'>",
    "message": "<error message, if any>",
    "data": <result of operation, if any>
}
```

If the operation was successful, the `status` field will be set to 'success' and the `data` field will contain the result of the operation (if any). If the operation was not successful, the `status` field will be set to 'error' and the `message` field will contain an error message.

The main start method of the `LobbyServer` class is responsible for starting the server loop, where it listens for incoming packets and spawns a new thread to handle each request. The `handle_client` method parses the incoming packet, calls the appropriate method on the `LobbyManager`, and sends a response back to the client.
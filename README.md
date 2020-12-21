# Karman Multiplayer Library

A C# library that provides networking logic for multiplayer games.

![Multiplayer](docs/multiplayer.jpg)

## Goal
The goal of this project is to make it easy to create multiplayer games in C#. The project can in theory be used in any framework or game engine, however note that it has been build and tested in the Unity3D game engine.

The project provides a small layer on top of the TCP networking protocol that makes it easy to synchronise game state across multiple game instances by sending data in pre-defined packets from and to servers and clients.

Current limitations and/or drawbacks:
- Currently under heavy development so the api might receive significant changes up until `version 1.0.0` is released.
- Additional work is required to get physics simulations up and running since the library doesn't handle latency for this.
- UDP protocol is not (yet) supported. It might be added at a later point in time. Currently there are no plans to add it. Once added it will provide a protocol with lower latency but without guarenteed package deliveree.
- The 'oracle and replicator' pattern has not yet been implemented. It will provide an easy way to setup different entity types to be synched across the server and clients.
- ... and more

A roadmap for the library can be found on this [trello board](https://trello.com/b/iQpvyCq5/multiplayer).

## Getting Started
To get started, either directly copy the source code of this library into your project or add the compiled dll(s) as a dependency to your project. The easiest setup to get started looks like this:
```csharp
// Define the game information
Guid gameId = Guid.Parse("<insert an random uuid that identifies your game here>");
string gameVersion = "0.0.1-alpha";

// Starting a server on port 14641
KarmanServer server = new KarmanServer("My First Server", gameId, gameVersion, "sup3r s3cr3t p4ssw0rd");
server.Start(14641)

// Connect to the server on the localmachine
Guid clientId = Guid.NewGuid();
Guid clientSecret = Guid.NewGuid();
KarmanClient client = new KarmanClient(clientId, clientSecret, "My First Client", gameId, gameVersion);
client.Start("localhost", 14641, "sup3r s3cr3t p4ssw0rd");

// Send a packet from the server to a single connected client or broadcast a packet to all connected clients
server.Send(packet);
server.Broadcast(packet);

// Send a packet from the client to the server
client.Send(packet);

// Receiving packets on the server and client with a callback
server.OnClientPacketReceived += (Guid clientId, Packet packet) => {
  /* do something with the packet from the client here*/
};
client.OnPacketReceived += (Packet packet) => {
  /* do something with the packet from the server here*/
};
```

> Note: The library is currently under development so the api might receive significant changes up until `version 1.0.0` is released. Backwards compatibility is not guarenteed up until that point.

## Layered Structure
The karman multiplayer library is build out of 3 layers. The first layer is TCP. The karman multiplayer library is build on top of tcp. The second layer is the KarmanProtocol layer. This layer abstract individual tcp connections and lets you work with clients. The third layer is your game layer, which can contain many different modules that use the events invoked by the KarmanProtocol layer. Some example modules are a PingModule to keep track of the latency of the connected clients

## Development Guide
The development guide explains how the karman multiplayer library can be used. First the tcp protocol and bytes arrays are explained. Then the packets that the library uses are explained. Then the KarmanServer is discussed in depth. After that the KarmanClient is discussed. And lastly the Oracle and Replicator pattern is explained.

### Byte arrays
> TODO: More information about byte arrays and TCP protocol coming soon.

Reference Article:
- [Sockets in C#](https://docs.microsoft.com/en-us/dotnet/framework/network-programming/sockets) in Microsoft Documentation

### Packets
> TODO: More information about packets and packet builder coming soon.

The client and server assume that the packet classes are immutable.

Reference Articles:
- [Message Framing](https://blog.stephencleary.com/2009/04/message-framing.html) by Stephen Cleary
- [Length Prefixed Message Framing](https://blog.stephencleary.com/2009/04/sample-code-length-prefix-message.html) by Stephen Cleary

### KarmanServer
Setting up a server is as easy as creating an instance of the `KarmanServer` class and then calling the `Start` method on it. To create the instance you have to provide a `gameId` for you server. This game id is unique for you project, and identifies the game that this server is running. You also need to provide the `gameVersion`, this identifies the version of the game, and will ensure that the client and server will run the same version of the game. You can optionally provide a password that the clients will need to provide when connecting.

```csharp
Guid gameId = Guid.Parse("<insert uuid here>");
string gameVersion = "0.0.1-alpha";
KarmanServer server = new KarmanServer("My First Server", gameId, gameVersion, "sup3r s3cr3t p4ssw0rd");
```

After creating the `KarmanServer` instance you can start the server at any time using the `Start(port)` method by providing the port on which the server should listen for incoming connections.

```csharp
int serverPort = 14641;
server.Start(serverPort);
```
> Ensure that you setup your port forwarding and dns configuration appropiatly to ensure that your clients can reach your server.

The server is build using event callbacks. Everytime something interesting happens, the server will invoke an event callback. The event callbacks can be used to execute logic when the server starts or shutsdown, clients join or leave the game, or packets from clients are received. All callbacks are executed on the same thread as was used to run the server start command. The following events exists and can be subscribed to.

```csharp
server.OnRunningCallback += () => {}; // Invoked when the server has start up successfully and is now running
server.OnShutdownCallback += () => {}; // Invoked when the server has shutdown
server.OnClientAcceptanceCallback; += (Action<string> reject) => { reject("Server is full"); }; // Invoked when a client wants to join, gives you the ability to reject clients from joining the server.
server.OnClientJoinedCallback += (Guid clientId, string clientName) => {}; // Invoked when a client joins the server
server.OnClientConnectedCallback += (Guid clientId) => {}; // Invoked when a client connects to the server
server.OnClientDisconnectedCallback += (Guid clientId) => {}; // Invoked when a client disconnects from the server
server.OnClientLeftCallback += (Guid clientId, String reason) => {}; // Invoked when a client leaves the server, including the reason why that client left
server.OnClientPackedReceivedCallback += (Guid clientId, Packet packet) => {}; // Invoked for each packet received from a client
```

A client can join and leave the server. A client uses a connection to connect to the server. During the period that a client is known at the server (from join to leave) it could potentially disconnect and connect multiple times. For example due to a bad network connection. A leave will only trigger if a client explicitly expresses to leave the server by sending a LeavePacket, when that happens all details about the client are removed from the server.

If you're developing your gaming using the karman multiplayer library you don't have to worry about individual connections. That is handled with in the KarmanProtocol layer, all you're dealing with are clients. Sometimes it is however interesting to know when a client has dropped its connection and when it reconnects. For example to show a message to the other clients that a client is currently not connected. For this reason the `OnClientConnected` and `OnClientDisconnected` callbacks are exposed.

After your server is up and running you can send packets to connected clients. You can send a packet to an individual client or broadcast a packet to all clients that are connected. Packets can only be sent to clients that are connected. Packets you sent to clients that are not connected are discarded, packets you send to clients that don't exist will throw an error.

```csharp
MyCoolPacket packet = new MyCoolPacket("awesome data!");
server.Send(clientId, packet); // Send a packet to an individual client
server.Broadcast(packet); // Send a packet to all connected clients
server.Broadcast(packet, skipClientId); // Send a packet to all connected clients except one
```

### KarmanClient
> TODO: More information about clients (incl. secrets, reconnection attempts, callbacks) coming soon.

If a client drops its connection it will attempt to reconnect using a new connection. As long as the client uses the same clientId and clientSecret it will be able to connect as a client already known on the server.

### Oracle and replicator pattern
> TODO: More information about the 'oracle and replicator'-pattern coming soon.

## Examples
A few different example implementations using the karman multiplayer library already exist.

- **Flow** - You can find one example implementation in this repository. It is called 'Flow'. It is a simple Unity demo showing of the capabilities of the library and it is also used during development of the library to test new functionality.

- **B11 Party Game** - A game that uses the karman multiplayer library. [B11 Party Game](https://www.simonkarman.nl/projects/b11-party) is a multiplayer game based on Mario Party with minigames themed around the 11th board of study association Sticky. It uses `version 0.1.0` of this libary and was used to prove the usefullness, stability, and speed of development of this library.

- **Elemental Arena** - A game that uses the karman multiplayer. [Elemental Areana](https://github.com/simonkarman/elemental-arena) is a multiplayer game under development by Simon Karman and Rik Dolfing, this game is what initiated the development of the Karman Multiplayer library.

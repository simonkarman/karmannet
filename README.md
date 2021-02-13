# KarmanNet

A C# framework that provides networking logic for multiplayer games.

![KarmanNet](docs/karmannet.jpg)

## Goal
The goal of this project is to make it easy to create multiplayer games in C#. KarmanNet is build on top of the Transmission Control Protocol (TCP). The framework provides functionality on top of TCP that make it easy to synchronise state across multiple game instances. It achieves this by sending data in pre-defined packets from and to server and clients.

A roadmap for the framework can be found on this [trello board](https://trello.com/b/iQpvyCq5/karmannet).

## Getting Started
To get started, either directly copy the source code of this framework into your project or add the compiled dll(s) as a dependency to your project. The easiest setup to get started looks like this:

```csharp
using KarmanNet.Protocol;

// Define the game information
Guid gameId = Guid.Parse("<insert an uuid that identifies your game here>");
string gameVersion = "0.0.1-alpha";

// Starting a server on port 14641
KarmanServer server = new KarmanServer("My First Server", gameId, gameVersion, "sup3r s3cr3t p4ssw0rd");
server.Start(14641);

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
server.OnClientPacketReceivedCallback += (Guid clientId, KarmanNet.Networking.Packet packet) => {
    /* do something with the packet from the client here */
};
client.OnPacketReceivedCallback += (KarmanNet.Networking.Packet packet) => {
    /* do something with the packet from the server here */
};
```

> Note: The framework is currently under development so the api might receive significant changes up until `version 1.0.0` is released. Backwards compatibility is not guarenteed up until that point. There is currently no indication for when or if version 1.0.0 is ever released.

## Limitations
As is the case with every framework, the KarmanNet framework is not suitable in every scenario. The following limitations give an overview of the scenarios where you might want to look for a networking framework other than KarmanNet:

- *No stable release* - KarmanNet has not been officially released. The api will receive significant changes at least up until version 1.0.0 is released. Backwards compatibility is not guarenteed up until that point. There is currently no indication for when or if version 1.0.0 is ever released.
- *Only for Unity3D\** - KarmanNet was build for the Unity3D game engine and has some references to the Unity3D engine. *Note that with some small effort the framework can be made game engine agnostic.
- *No physics simulation support* - KarmanNet does not contain a build in distributed physics simulator. Currently all physics simulations should be performed on the server, physics should not affect the gameplay across clients, or physics should be avoided.
- *No P2P* - KarmanNet does not offer support for peer to peer (P2P) connections. A server is required for the network communication. This means you need to setup your port forwarding and dns configuration appropiatly to ensure that your clients can reach your server.
- *No UDP* - KarmanNet runs soly on the TCP protocol. TCP is slightly slower than using an UDP connection, however it gives us higher reliablity in package delivery and guarentees the order of packages. Currently there are no plans to add support for UDP.
- *Minimal Test Coverage* - KarmanNet test coverage is currently minimal. Only the core byte packaging classes are unit tested.

## Reference Projects
The KarmanNet framework is already being used in the following projects. You can use these projects as a reference.

- **Flow** - Example project included in this repository. Flow is a simple technical demo showing of the capabilities of the framework. The demo is used during the development of the framework to test new functionality.
- **B11 Party Game** - A game that uses KarmanNet. [B11 Party Game](https://www.simonkarman.nl/projects/b11-party) is a multiplayer game based on Mario Party with minigames themed around the 11th board of study association Sticky. It uses `version 0.1.0` of this framework and was used to prove the usefullness, stability, and speed of development of KarmanNet.
- **Elemental Arena** - A game that uses the karman multiplayer. [Elemental Areana](https://github.com/simonkarman/elemental-arena) is a multiplayer game under development by Simon Karman and Rik Dolfing, this game is what initiated the development of the KarmanNet framework. It heavily relies on the Karmax feature of KarmanNet.

# Development Guide
The development guide explains how KarmanNet can be used. KarmanNet is build out of multiple packages. Each package has its own dll file. The following packages are currently available.

  1. **KarmanNet.Networking.dll** - This package is responsible for handling the raw TCP connections. It also provides means to transform classes to bytes to send over these connections.
  2. **KarmanNet.Protocol.dll** - This package is responsible for the protocol that the server and client use to talk to eachother. It hides the raw TCP connections and provides an interface to the clients, regardless of their connection. It also provides common functionality for the server to accept/reject clients, passwords, version verification, and more.
  3. **KarmanNet.Karmax.ddl** - This package provides a state container that is kept in sync across the server and clients. The state is a single source of thurth, that all parties can trust. It provides ways for the server and clients to safely mutate the state. It is inspired by libraries such as Axon (Java Framework) and React (TypeScript FrontEnd Framework).
  4. **KarmanNet.Logging.dll** - This package provides an logging framework that is used in the other packages.



## KarmanNet.Networking
This package is responsible for handling the raw TCP connections. It also provides means to transform classes to bytes to send over these connections.

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



## KarmanNet.Protocol
This package is responsible for the protocol that the server and client use to talk to eachother. It hides the raw TCP connections and provides an interface to the clients, regardless of their connection. It also provides common functionality for the server to accept/reject clients, passwords protection, build version verification, and more.

### KarmanServer
KarmanServer provides the interface to start and use the KarmanNet.Protocol on the server of your game. The below section describe how to construct, start, use callbacks, send packets, kick clients, and shutdown the server.

#### Constructing
Setting up a server is as easy as creating an instance of the `KarmanServer` class and then calling the `Start` method on it. To create the instance you have to provide a `gameId` for you server. This game id is unique for you project, and identifies the game that this server is running. You also need to provide the `gameVersion`, this identifies the version of the game, and will ensure that the client and server will run the same version of the game. You can optionally provide a password that the clients will need to provide when connecting.

```csharp
Guid gameId = Guid.Parse("<insert an uuid that identifies your game here>");
string gameVersion = "0.0.1-alpha";
KarmanServer server = new KarmanServer("My First Server", gameId, gameVersion, "sup3r s3cr3t p4ssw0rd");
```

#### Starting
After creating the `KarmanServer` instance you can start the server at any time using the `Start(port)` method by providing the port on which the server should listen for incoming connections.

```csharp
int serverPort = 14641;
server.Start(serverPort);
```
> You need to manually setup port forwarding and dns configuration appropiatly to ensure that your clients can reach your server.

#### Callbacks
The server is build using event callbacks. Everytime something interesting happens, the server will invoke an event callback. The event callbacks can be used to execute logic when the server starts or shutsdown, clients join or leave the game, or packets from clients are received. All callbacks are executed on the same thread as was used to run the server start command. The following events exists and can be subscribed to.

```csharp
// Invoked when the server has start up successfully and is running
server.OnRunningCallback += () => { };

// Invoked when the server has shutdown
server.OnShutdownCallback += () => { };

// Invoked when a client wants to join, gives you the ability to reject clients joining the server.
server.OnClientAcceptanceCallback += (Action<string> reject) => { reject("Server is full"); };

// Invoked when a client joins the server
server.OnClientJoinedCallback += (Guid clientId, string clientName) => { };

// Invoked when a client connects to the server
server.OnClientConnectedCallback += (Guid clientId) => { };

// Invoked when a client disconnects from the server
server.OnClientDisconnectedCallback += (Guid clientId) => { };

// Invoked when a client leaves the server, including the reason why that client left
server.OnClientLeftCallback += (Guid clientId, String reason) => { };

// Invoked for each packet received from a client
server.OnClientPacketReceivedCallback += (Guid clientId, KarmanNet.Networking.Packet packet) => { };
```

A client can join and leave the server. A client uses a connection to connect to the server. During the period that a client is known at the server (from join to leave) it could potentially disconnect and connect multiple times. For example due to a bad network connection. A leave will only trigger if a client explicitly expresses to leave the server by sending a LeavePacket, when that happens all details about the client are removed from the server.

If you're developing your gaming using KarmanNet you don't have to worry about TCP connections. That is handled within the KarmanNet.Protocol layer, all you're dealing with are clients. Sometimes it is however interesting to know when a client has dropped its connection and when it reconnects. For example to show a message to the other clients that a client is currently not connected. For this reason the `OnClientConnectedCallback` and `OnClientDisconnectedCallback` properties are exposed.

#### Sending Packets
After your server is up and running you can send packets to connected clients. You can send a packet to an individual client or broadcast a packet to all clients that are connected. Packets can only be sent to clients that are connected. Packets you sent to clients that are not connected are discarded, packets you send to clients that don't exist will throw an error.

```csharp
MessagePacket packet = new MessagePacket("Hello World!");
server.Send(clientId, packet); // Send a packet to an individual client
server.Broadcast(packet); // Send a packet to all connected clients
server.Broadcast(packet, skipClientId); // Send a packet to all connected clients except one
```

#### Kicking clients
> TODO: More information about kicking clients coming soon.

#### Shutting down
> TODO: More information about shutting down the server coming soon.

### KarmanClient
> TODO: More information about clients (incl. secrets, reconnection attempts, callbacks) coming soon.

If a client drops its connection, it will attempt to reconnect using a new connection. As long as the client uses the same clientId, clientName and clientSecret it will be able to reconnect as a client already known on the server.



## KarmanNet.Karmax
This package provides a state container that is kept in sync across the server and clients. The state is a single source of thurth, that all parties can trust. Karmax provides ways for the server and clients to safely mutate the state. It is inspired by libraries such as [Axon](https://axoniq.io/) (Java framework for event-driven microservices) and [Redux](https://redux.js.org/) (a predictable state container for JavaScript apps).

### Oracle and Replicators
> TODO: More information about the 'oracle and replicators'-pattern coming soon.



## KarmanNet.Logging
This package provides an logging framework that is used in the other packages

> TODO: More information on logging and creating custom LogAppenders coming soon.
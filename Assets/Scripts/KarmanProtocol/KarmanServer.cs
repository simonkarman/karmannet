using Logging;
using Networking;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KarmanProtocol {
    public class KarmanServer {
        private static readonly Logger log = Logger.For<KarmanServer>();

        public const string KARMAN_PROTOCOL_VERSION = "0.3.1";
        public const string DEFAULT_PASSWORD = "";

        private class Client {
            private readonly Guid clientId;
            private readonly Guid clientSecret;
            private readonly string clientName;
            private Guid connectionId;

            public Client(Guid clientId, string clientName, Guid clientSecret) {
                this.clientId = clientId;
                this.clientName = clientName;
                this.clientSecret = clientSecret;
            }

            public Guid GetClientId() {
                return clientId;
            }

            public string GetClientName() {
                return clientName;
            }

            public void RemoveConnectionId() {
                connectionId = Guid.Empty;
            }

            public bool TryTakeOverConnection(Guid connectionId, Guid clientSecret, string clientName) {
                if (!clientSecret.Equals(this.clientSecret) || !clientName.Equals(this.clientName)) {
                    return false;
                }
                this.connectionId = connectionId;
                return true;
            }

            public Guid GetConnectionId() {
                return connectionId;
            }

            public bool IsConnected() {
                return connectionId != Guid.Empty;
            }
        }

        public readonly Guid id;
        public readonly string name;
        public readonly Guid gameId;
        public readonly string gameVersion;

        private readonly string password;
        private readonly Server server;
        private readonly Dictionary<Guid, Guid> connections = new Dictionary<Guid, Guid>();
        private readonly Dictionary<Guid, Client> clients = new Dictionary<Guid, Client>();

        public Action OnRunningCallback;
        public Action OnShutdownCallback;
        public Action<Action<string>> OnClientAcceptanceCallback;
        public Action<Guid, string> OnClientJoinedCallback;
        public Action<Guid> OnClientConnectedCallback;
        public Action<Guid> OnClientDisconnectedCallback;
        public Action<Guid, string> OnClientLeftCallback;
        public Action<Guid, Packet> OnClientPacketReceivedCallback;

        public KarmanServer(string name, Guid gameId, string gameVersion, string password = DEFAULT_PASSWORD) {
            id = Guid.NewGuid();
            this.name = name;
            this.gameId = gameId;
            this.gameVersion = gameVersion;
            this.password = password ?? DEFAULT_PASSWORD;

            server = new Server();
            server.OnRunningCallback += () => SafeInvoker.Invoke(log, "OnRunningCallback", OnRunningCallback);
            server.OnShutdownCallback += () => SafeInvoker.Invoke(log, "OnShutdownCallback", OnShutdownCallback);
            server.OnConnectedCallback += OnConnected;
            server.OnDisconnectedCallback += OnDisconnected;
            server.OnPacketReceivedCallback += OnPacketReceived;
        }

        public void Start(int port) {
            server.Start(port);
        }

        public bool IsRunning() {
            return server.Status == ServerStatus.RUNNING;
        }

        public int GetClientCount() {
            return clients.Count;
        }

        private void OnConnected(Guid connectionId) {
            log.Info("Connection {0} connected", connectionId);

            connections.Add(connectionId, Guid.Empty);
            ServerInformationPacket serverInformationPacket = new ServerInformationPacket(id, name, gameId, gameVersion, KARMAN_PROTOCOL_VERSION);
            server.Send(connectionId, serverInformationPacket);
        }

        private void OnDisconnected(Guid connectionId) {
            log.Info("Connection {0} disconnected", connectionId);
            if (!connections.TryGetValue(connectionId, out Guid clientId)) {
                throw log.ExitError(new ConnectionNotFoundException(connectionId));
            }
            connections.Remove(connectionId);

            if (clientId == Guid.Empty) {
                log.Info("Connection {0} has successfully disconnected (it was no longer / not yet connected to a client)", connectionId);
                return;
            }

            if (!clients.TryGetValue(clientId, out Client client)) {
                throw log.ExitError(new Exception(string.Format("Cannot disconnect connection {0} from client {1} because that client does not exist", connectionId, clientId)));
            }

            if (client.GetConnectionId() == connectionId) {
                log.Warning("Connection {0} dropped while it was still connected to client {1} (client is still available for reconnection attempts)", connectionId, clientId);
                client.RemoveConnectionId();
                SafeInvoker.Invoke(log, "OnClientDisconnectedCallback", OnClientDisconnectedCallback, clientId);

            } else {
                log.Warning("Connection {0} that disconnected was used for client {1}, but that client is already using a new connection {2}", connectionId, clientId, client.GetConnectionId());
            }
        }

        private void OnPacketReceived(Guid connectionId, Packet packet) {
            if (!connections.TryGetValue(connectionId, out Guid clientId)) {
                server.Disconnect(connectionId);
                throw log.ExitError(new Exception(string.Format("Cannot handle packet for connection {0} because that connection does not exist", connectionId)));
            }

            if (packet is MessagePacket messagePacket) {
                log.Info("Connection {0} (client={1}) says: {2}", connectionId, clientId == Guid.Empty ? "<none>" : clientId.ToString(), messagePacket.GetMessage());
                return;
            }

            if (packet is ClientInformationPacket clientInformationPacket) {
                if (clientId != Guid.Empty) {
                    throw log.ExitError(new Exception(string.Format("Connection {0} cannot create a new client {1} because the connection already points to client {2}", connectionId, clientInformationPacket.GetClientId(), clientId)));
                }

                if (!password.Equals(clientInformationPacket.GetServerPassword())) {
                    if (password.Equals(string.Empty)) {
                        log.Warning("Connection {0} provided a server password for a server that does not require a password to join. The provided password is ignored.", connectionId, clientId);
                    } else {
                        log.Warning("Connection {0} provided an incorrect server password while trying to create client {1}", connectionId, clientId);
                        server.Send(connectionId, new LeavePacket("Server password incorrect"));
                        server.Disconnect(connectionId);
                        return;
                    }
                }

                clientId = clientInformationPacket.GetClientId();
                string clientName = clientInformationPacket.GetClientName();
                Guid previousConnectionId = Guid.Empty;
                bool newPlayer = false;
                if (clients.TryGetValue(clientId, out Client connectedClient)) {
                    log.Info("Connection {0} is taking over an already existing client {1}", connectionId, clientId);
                    previousConnectionId = connectedClient.GetConnectionId();
                } else {
                    Guid clientSecret = clientInformationPacket.GetClientSecret();
                    log.Info("Connection {0} is creating a new client {1} (secret {2}-**...)", connectionId, clientId, clientSecret.ToString().Substring(0, 13));

                    List<string> rejections = new List<string>();
                    SafeInvoker.Invoke(log, "OnClientAcceptanceCallback", OnClientAcceptanceCallback, (rejection) => rejections.Add(rejection));
                    if (rejections.Count > 0) {
                        string reason = string.Join(", ", rejections);
                        log.Warning("Connection {0} was rejected trying to create client {1}. Reason: {2}", connectionId, clientId, reason);
                        server.Send(connectionId, new LeavePacket(reason));
                        server.Disconnect(connectionId);
                        return;
                    }

                    connectedClient = new Client(clientId, clientName, clientSecret);
                    clients.Add(clientId, connectedClient);
                    newPlayer = true;
                }
                if (connectedClient.TryTakeOverConnection(connectionId, clientInformationPacket.GetClientSecret(), clientName)) {
                    if (previousConnectionId != Guid.Empty) {
                        log.Info("Disconnecting previous connection {0}, since connection {1} is taking over a client {2}", previousConnectionId, connectionId, clientId);
                        connections[previousConnectionId] = Guid.Empty;
                        server.Send(previousConnectionId, new LeavePacket("You connected from another device."));
                        server.Disconnect(previousConnectionId);
                    }
                    connections[connectionId] = clientId;
                    server.Send(connectionId, new ClientAcceptedPacket());
                    log.Info("Client {0} accepted on connection {1}", clientId, connectionId);
                    if (newPlayer) {
                        SafeInvoker.Invoke(log, "OnClientJoinedCallback", OnClientJoinedCallback, clientId, clientName);
                    }
                    if (clients.ContainsKey(clientId)) {
                        SafeInvoker.Invoke(log, "OnClientConnectedCallback", OnClientConnectedCallback, clientId);
                    } else {
                        log.Info("Client {0} was kicked while it was joining the server", clientId);
                    }
                    return;
                } else {
                    log.Warning("Aborted connection {0} taking over client {1} since an incorrect secret was provided or the client name is no longer the same", connectionId, clientId);
                    server.Send(connectionId, new LeavePacket("Client secret or name incorrect"));
                    server.Disconnect(connectionId);
                    return;
                }
            }

            if (!clients.TryGetValue(clientId, out Client client)) {
                server.Disconnect(connectionId);
                throw log.ExitError(new Exception(string.Format("Cannot handle a {0} packet for connection {1} because the client {2} that is used for that connection does not exist", packet.GetType().Name, connectionId, clientId)));
            }
            log.Trace("Received a {0} packet for client {1}", packet.GetType().Name, clientId);

            if (packet is LeavePacket leavePacket) {
                log.Info("Client left. Reason: {0}", leavePacket.GetReason());
                Kick(clientId, "Client left");

            } else {
                SafeInvoker.Invoke(log, "OnClientPacketReceivedCallback", OnClientPacketReceivedCallback, clientId, packet);
            }
        }

        public void Shutdown() {
            foreach (Guid clientId in new List<Client>(clients.Values).Select(client => client.GetClientId())) {
                Kick(clientId, "Server shutdown");
            }
            server.Shutdown();
        }

        public void Kick(Guid clientId, string reason) {
            log.Info("Kicking client {0}, all information about the client will be removed. Reason: {1}", clientId, reason);
            if (!clients.TryGetValue(clientId, out Client client)) {
                log.Warning("Cannot kick client {0}, because that client does not exist", clientId);
                return;
            }
            clients.Remove(clientId);

            Guid connectionId = client.GetConnectionId();
            if (connectionId != Guid.Empty) {
                connections[connectionId] = Guid.Empty;
                try {
                    server.Send(connectionId, new LeavePacket(reason));
                    System.Threading.Thread.Sleep(100); // TODO: fix this.
                    server.Disconnect(connectionId);
                } catch (Exception ex) {
                    log.Warning("Connection {0} of client {1} could not be disconnected, due to the following reason: {2}", connectionId, clientId, ex);
                }
            }
            SafeInvoker.Invoke(log, "OnClientLeftCallback", OnClientLeftCallback, clientId, reason);
        }

        public void Broadcast(Packet packet, Guid exceptClientId = default) {
            foreach (var client in clients.Values) {
                if (!client.IsConnected() || client.GetClientId() == exceptClientId) {
                    continue;
                }
                server.Send(client.GetConnectionId(), packet);
            }
        }

        public void Send(Guid clientId, Packet packet) {
            if (!clients.TryGetValue(clientId, out Client client)) {
                throw log.ExitError(new Exception(string.Format("Cannot send a message to client {0}, because that client does not exist", clientId)));
            }
            if (!client.IsConnected()) {
                return;
            }
            server.Send(client.GetConnectionId(), packet);
        }
    }
}

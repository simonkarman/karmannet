using Logging;
using Networking;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KarmanProtocol {
    public class KarmanServer {
        private static readonly Logger log = Logger.For<KarmanServer>();

        public const string PROTOCOL_VERSION = "0.1.0";

        private class Client {
            private readonly Guid clientId;
            private readonly Guid clientSecret;
            private Guid connectionId;

            public Client(Guid clientId, Guid clientSecret) {
                this.clientId = clientId;
                this.clientSecret = clientSecret;
            }

            public Guid GetClientId() {
                return clientId;
            }

            public void RemoveConnectionId() {
                connectionId = Guid.Empty;
            }

            public bool TrySetConnectionId(Guid connectionId, Guid clientSecret) {
                if (!clientSecret.Equals(this.clientSecret)) {
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
        public readonly Guid gameId;

        private readonly Server server;
        private readonly Dictionary<Guid, Guid> connections = new Dictionary<Guid, Guid>();
        private readonly Dictionary<Guid, Client> clients = new Dictionary<Guid, Client>();

        public Action OnRunningCallback;
        public Action OnShutdownCallback;
        public Action<Guid> OnClientJoinedCallback;
        public Action<Guid> OnClientConnectedCallback;
        public Action<Guid> OnClientDisconnectedCallback;
        public Action<Guid> OnClientLeftCallback;
        public Action<Guid, Packet> OnClientPackedReceivedCallback;

        public KarmanServer(Guid gameId) {
            id = Guid.NewGuid();
            this.gameId = gameId;

            server = new Server();
            server.OnRunningCallback += OnRunning;
            server.OnShutdownCallback += OnShutdown;
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

        private void OnRunning() {
            OnRunningCallback();
        }

        private void OnShutdown() {
            OnShutdownCallback();
        }

        private void OnConnected(Guid connectionId) {
            log.Info("Connection {0} connected", connectionId);

            connections.Add(connectionId, Guid.Empty);
            ServerInformationPacket serverInformationPacket = new ServerInformationPacket(id, gameId, PROTOCOL_VERSION);
            server.Send(connectionId, serverInformationPacket);
        }

        private void OnDisconnected(Guid connectionId) {
            log.Info("Connection {0} disconnected", connectionId);
            if (!connections.TryGetValue(connectionId, out Guid clientId)) {
                throw log.ExitError(new ConnectionNotFoundException(connectionId));
            }
            connections.Remove(connectionId);

            if (clientId == Guid.Empty) {
                log.Info("Connection {0} has successfully disconnected (because it was no longer / not yet connected to a client)", connectionId);
                return;
            }

            if (!clients.TryGetValue(clientId, out Client client)) {
                throw log.ExitError(new Exception(string.Format("Cannot disconnect connection {0} from client {1} because that client does not exist", connectionId, clientId)));
            }

            if (client.GetConnectionId() == connectionId) {
                log.Warning("Connection {0} dropped while it was still connected to client {1} (client is still available for reconnection attempts)", connectionId, clientId);
                client.RemoveConnectionId();

            } else {
                log.Warning("Connection {0} that disconnected was used for client {1}, but that client is already using a new connection {2}", connectionId, clientId, client.GetConnectionId());
            }
            OnClientDisconnectedCallback(clientId);
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
                clientId = clientInformationPacket.GetClientId();
                Guid previousConnectionId = Guid.Empty;
                bool newPlayer = false;
                if (clients.TryGetValue(clientId, out Client connectedClient)) {
                    log.Info("Connection {0} is taking over an already existing client {1}", connectionId, clientId);
                    previousConnectionId = connectedClient.GetConnectionId();
                } else {
                    Guid clientSecret = clientInformationPacket.GetClientSecret();
                    log.Info("Connection {0} is creating a new client {1} (secret {2}-**...)", connectionId, clientId, clientSecret.ToString().Substring(0, 13));
                    connectedClient = new Client(clientId, clientSecret);
                    clients.Add(clientId, connectedClient);
                    newPlayer = true;
                }
                if (connectedClient.TrySetConnectionId(connectionId, clientInformationPacket.GetClientSecret())) {
                    if (previousConnectionId != Guid.Empty) {
                        log.Info("Disconnecting previous connection {0}, since connection {1} is taking over a client {2}", previousConnectionId, connectionId, clientId);
                        server.Disconnect(previousConnectionId);
                    }
                    connections[connectionId] = clientId;
                    log.Info("Client {0} now uses connection {1}", clientId, connectionId);
                    if (newPlayer) {
                        OnClientJoinedCallback(clientId);
                    }
                    OnClientConnectedCallback(clientId);
                } else {
                    log.Warning("Aborted connection {0} taking over client {1} since an invalid secret was provided", connectionId, clientId);
                }
                return;
            }

            if (!clients.TryGetValue(clientId, out Client client)) {
                server.Disconnect(connectionId);
                throw log.ExitError(new Exception(string.Format("Cannot handle a {0} packet for connection {1} because the client {2} that is used for that connection does not exist", packet.GetType().Name, connectionId, clientId)));
            }
            log.Trace("Received a {0} packet for client {1}", packet.GetType().Name, clientId);

            if (packet is LeavePacket) {
                Kick(clientId);

            } else {
                OnClientPackedReceivedCallback(clientId, packet);
            }
        }

        public void Shutdown() {
            foreach (Guid clientId in new List<Client>(clients.Values).Select(client => client.GetClientId())) {
                Kick(clientId);
            }
            server.Shutdown();
        }

        public void Kick(Guid clientId) {
            log.Info("Kicking client {0}, all information about the client will be removed", clientId);
            if (!clients.TryGetValue(clientId, out Client client)) {
                log.Warning("Cannot kick client {0}, because that client does not exist", clientId);
                return;
            }
            clients.Remove(clientId);

            Guid connectionId = client.GetConnectionId();
            if (connectionId != Guid.Empty) {
                connections[connectionId] = Guid.Empty;
                try {
                    server.Send(connectionId, new LeavePacket());
                    System.Threading.Thread.Sleep(100); // TODO: fix this.
                    server.Disconnect(connectionId);
                } catch (Exception ex) {
                    log.Warning("Connection {0} of client {1} could not be disconnected, due to the following reason: {2}", connectionId, clientId, ex);
                }
            }
            OnClientLeftCallback(clientId);
        }

        public void Broadcast(Packet packet, Guid exceptClientId = default) {
            foreach (var client in clients.Values) {
                if (client.GetConnectionId() == Guid.Empty || client.GetClientId() == exceptClientId) {
                    continue;
                }
                server.Send(client.GetConnectionId(), packet);
            }
        }

        public void Send(Guid clientId, Packet packet) {
            if (!clients.TryGetValue(clientId, out Client client)) {
                throw log.ExitError(new Exception(string.Format("Cannot send a message to client {0}, because that client does not exist", clientId)));
            }
            server.Send(client.GetConnectionId(), packet);
        }
    }
}

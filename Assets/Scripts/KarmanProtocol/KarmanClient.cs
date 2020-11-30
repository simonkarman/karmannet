using Logging;
using Networking;
using System;
using System.Net;

namespace KarmanProtocol {
    public class KarmanClient {
        private static readonly Logger log = Logger.For<KarmanClient>();

        private Client client;

        public readonly Guid id;
        public readonly Guid gameId;
        public readonly Guid secret;
        private IPEndPoint endpoint;
        private bool hasJoined = false;
        private bool left = false;
        private bool reconnectionAttempt = false;

        public Action OnJoinedCallback;
        public Action OnConnectedCallback;
        public Action OnDisconnectedCallback;
        public Action<string> OnLeftCallback;
        public Action<Packet> OnPacketReceivedCallback;

        public KarmanClient(Guid id, Guid gameId, Guid secret) {
            this.id = id;
            this.gameId = gameId;
            this.secret = secret;
            SetupClient();
        }

        private void SetupClient() {
            client = new Client();
            client.OnConnectedCallback += OnConnected;
            client.OnDisconnectedCallback += OnDisconnected;
            client.OnPacketReceivedCallback += OnPacketReceived;
        }

        public void Start(string connectionString, int defaultPort) {
            log.Info(
                "KarmanClient: Starting KarmanClient with connectionString={0} (defaultPort={1}), clientId={2}",
                connectionString, defaultPort, id
            );
            endpoint = ConnectionString.Parse(connectionString, defaultPort);
            client.Start(endpoint);
        }

        private void OnConnected() {
            if (!hasJoined) {
                hasJoined = true;
                SafeInvoker.Invoke(log, "OnJoinedCallback", OnJoinedCallback);
            }
            reconnectionAttempt = false;
            SafeInvoker.Invoke(log, "OnConnectedCallback", OnConnectedCallback);
        }

        private void OnDisconnected() {
            SafeInvoker.Invoke(log, "OnDisconnectedCallback", OnDisconnectedCallback);
            if (reconnectionAttempt) {
                log.Error("Client was unable to reconnect to the server, server will now be left");
                Leave("Reconnection failed");
                return;
            }
            if (!hasJoined) {
                string message = "Client was unable to connect to the server";
                log.Error(message);
                SafeInvoker.Invoke(log, "OnLeftCallback", OnLeftCallback, message);
                return;
            }
            if (!left) {
                log.Warning("Connection with server lost, immediately trying to reconnect");
                reconnectionAttempt = true;
                SetupClient();
                client.Start(endpoint);
            }
        }

        public bool IsConnected() {
            return client.Status == ConnectionStatus.CONNECTED;
        }

        private void OnPacketReceived(Packet packet) {
            if (packet is MessagePacket messagePacket) {
                log.Info("Server says: {0}", messagePacket.GetMessage());

            } else if (packet is ServerInformationPacket serverInformationPacket) {
                log.Info(
                    "Server send its information serverId={0} and protocolVersion={1}",
                    serverInformationPacket.GetServerId(), serverInformationPacket.GetProtocolVersion()
                );
                if (KarmanServer.PROTOCOL_VERSION != serverInformationPacket.GetProtocolVersion()) {
                    log.Warning(
                        "Leaving server since it uses a different protocol version ({0}) than the client ({1})",
                        serverInformationPacket.GetProtocolVersion(), KarmanServer.PROTOCOL_VERSION
                    );
                    Leave("Protocol mismatch");
                } else if (!gameId.Equals(serverInformationPacket.GetGameId())) {
                    log.Warning(
                        "Leaving server since server is build for a different game id ({0}) than the client ({1})",
                        serverInformationPacket.GetGameId(), gameId
                    );
                    Leave("Game mismatch");
                } else {
                    ClientInformationPacket provideUsernamePacket = new ClientInformationPacket(id, secret);
                    client.Send(provideUsernamePacket);
                }

            } else if (packet is LeavePacket leavePacket) {
                string message = string.Format("Kicked by server. Reason: {0}", leavePacket.GetReason());
                log.Info(message);
                Leave(message);

            } else {
                log.Trace("Received a {0} packet from server", packet.GetType().Name);
                SafeInvoker.Invoke(log, "OnPacketReceivedCallback", OnPacketReceivedCallback, packet);
            }
        }

        public void Leave(string reason) {
            log.Info("Leaving server. Reason: {0}", reason);
            if (left) {
                log.Warning("Client cannot leave the server if it already left");
                return;
            }
            left = true;
            try {
                client.Send(new LeavePacket(reason));
                System.Threading.Thread.Sleep(100); // TODO: fix this.
                client.Disconnect();
            } catch (Exception ex) {
                log.Warning("Connection with server could not be disconnected, due to the following reason: {0}", ex);
            }
            SafeInvoker.Invoke(log, "OnLeftCallback", OnLeftCallback, reason);
        }

        public void Send(Packet packet) {
            client.Send(packet);
        }
    }
}

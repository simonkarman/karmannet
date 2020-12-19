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

        private Guid serverId;
        private IPEndPoint endpoint;
        private bool hasJoined = false;
        private bool hasLeft = false;
        private int reconnectionAttempt = -1;

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

        public Guid GetServerInformation() {
            if (!IsConnected()) {
                return Guid.Empty;
            }
            return serverId;
        }

        private void SetupClient() {
            client = new Client();
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

        private void OnDisconnected() {
            SafeInvoker.Invoke(log, "OnDisconnectedCallback", OnDisconnectedCallback);
            if (reconnectionAttempt >= 3) {
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
            if (!hasLeft) {
                reconnectionAttempt += 1;
                log.Warning("Connection with server lost, immediately trying to reconnect (try {0} out of 3)", reconnectionAttempt + 1);
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
                    "Received information from the server: gameId={0}, serverId={1}, and protocolVersion={2}",
                    serverInformationPacket.GetGameId(), serverInformationPacket.GetServerId(), serverInformationPacket.GetProtocolVersion()
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
                    reconnectionAttempt = -1;
                    serverId = serverInformationPacket.GetServerId();

                    ClientInformationPacket provideUsernamePacket = new ClientInformationPacket(id, secret);
                    client.Send(provideUsernamePacket);

                    if (!hasJoined) {
                        hasJoined = true;
                        SafeInvoker.Invoke(log, "OnJoinedCallback", OnJoinedCallback);
                    }
                    SafeInvoker.Invoke(log, "OnConnectedCallback", OnConnectedCallback);
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
            if (hasLeft) {
                log.Warning("Client cannot leave the server if it already left");
                return;
            }
            hasLeft = true;
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

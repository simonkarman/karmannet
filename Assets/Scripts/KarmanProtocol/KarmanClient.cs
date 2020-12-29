using Logging;
using Networking;
using System;
using System.Net;

namespace KarmanProtocol {
    public class KarmanClient {
        private static readonly Logger log = Logger.For<KarmanClient>();

        private Client client;

        public readonly Guid id;
        public readonly Guid secret;
        public readonly string name;
        public readonly Guid gameId;
        public readonly string gameVersion;

        private string serverPassword;
        private ServerInformationPacket serverInformation;
        private IPEndPoint endpoint;
        private bool hasBeenConnectedAtLeastOnce = false;
        private bool hasJoined = false;
        private bool hasLeft = false;
        private int reconnectionAttempt = -1;

        public Action OnJoinedCallback;
        public Action OnConnectedCallback;
        public Action OnDisconnectedCallback;
        public Action<string> OnLeftCallback;
        public Action<Packet> OnPacketReceivedCallback;

        public KarmanClient(Guid id, Guid secret, string name, Guid gameId, string gameVersion) {
            this.id = id;
            this.secret = secret;
            this.name = name;
            this.gameId = gameId;
            this.gameVersion = gameVersion;
            SetupClient();
        }

        public ServerInformationPacket GetServerInformation() {
            return serverInformation;
        }

        private void SetupClient() {
            client = new Client();
            client.OnConnectedCallback += OnConnected;
            client.OnDisconnectedCallback += OnDisconnected;
            client.OnPacketReceivedCallback += OnPacketReceived;
        }

        public void Start(string connectionString, int defaultPort, string serverPassword = KarmanServer.DEFAULT_PASSWORD) {
            string usePassword = string.IsNullOrEmpty(serverPassword) ? "NO" : "YES";
            log.Info(
                "KarmanClient: Starting KarmanClient with connectionString={0} (defaultPort={1}), clientId={2}, usePassword={3})",
                connectionString, defaultPort, id, usePassword
            );
            this.serverPassword = serverPassword ?? KarmanServer.DEFAULT_PASSWORD;
            endpoint = ConnectionString.Parse(connectionString, defaultPort);
            client.Start(endpoint);
        }

        private void OnConnected() {
            hasBeenConnectedAtLeastOnce = true;
        }

        private void OnDisconnected() {
            SafeInvoker.Invoke(log, "OnDisconnectedCallback", OnDisconnectedCallback);
            if (reconnectionAttempt >= 3) {
                log.Error("Client was unable to reconnect to the server, server will now be left");
                Leave("Reconnection failed");
                return;
            }
            if (!hasBeenConnectedAtLeastOnce) {
                string message = "Client was unable to connect to the server";
                log.Error(message);
                hasLeft = true;
                SafeInvoker.Invoke(log, "OnLeftCallback", OnLeftCallback, message);
                return;
            }
            if (!hasLeft) {
                reconnectionAttempt += 1;
                log.Warning("Connection with server lost, immediately trying to reconnect (try {0} out of 3)", reconnectionAttempt + 1);
                hasBeenConnectedAtLeastOnce = false;
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
                    "Received information from the server: serverId={0}, serverName={1}, gameId={2}, gameVersion={3}, and karmanProtocolVersion={4}",
                    serverInformationPacket.GetServerId(), serverInformationPacket.GetServerName(),
                    serverInformationPacket.GetGameId(), serverInformationPacket.GetGameVersion(),
                    serverInformationPacket.GetKarmanProtocolVersion()
                );
                if (!gameId.Equals(serverInformationPacket.GetGameId())) {
                    log.Warning(
                        "Leaving server since server is running a different game ({0}) than the client ({1}).",
                        serverInformationPacket.GetGameId(), gameId
                    );
                    Leave("Game mismatch");
                } else if (!gameVersion.Equals(serverInformationPacket.GetGameVersion())) {
                    log.Warning(
                        "Leaving server since it is running a different game version ({0}) than the client ({1})",
                        serverInformationPacket.GetGameVersion(), gameVersion
                    );
                    Leave("Game version mismatch");
                } else if (KarmanServer.KARMAN_PROTOCOL_VERSION != serverInformationPacket.GetKarmanProtocolVersion()) {
                    log.Warning(
                        "Leaving server since it uses a different karman protocol version ({0}) than the client ({1})",
                        serverInformationPacket.GetKarmanProtocolVersion(), KarmanServer.KARMAN_PROTOCOL_VERSION
                    );
                    Leave("Karman protocol mismatch");
                } else {
                    reconnectionAttempt = -1;
                    serverInformation = serverInformationPacket;
                    ClientInformationPacket clientInformationPacket = new ClientInformationPacket(id, name, secret, serverPassword);
                    client.Send(clientInformationPacket);
                }

            } else if (packet is ClientAcceptedPacket clientAcceptedPacket) {
                log.Info("Server acceptected");

                if (!hasJoined) {
                    hasJoined = true;
                    SafeInvoker.Invoke(log, "OnJoinedCallback", OnJoinedCallback);
                }
                SafeInvoker.Invoke(log, "OnConnectedCallback", OnConnectedCallback);

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

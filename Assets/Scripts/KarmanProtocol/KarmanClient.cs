using Networking;
using System;
using System.Net;
using UnityEngine;

namespace KarmanProtocol {
    public class KarmanClient {

        private Client client;

        public readonly Guid id;
        public readonly Guid secret;
        private IPEndPoint endpoint;
        private bool hasJoined = false;
        private bool left = false;
        private bool reconnectionAttempt = false;

        public Action OnJoinedCallback;
        public Action OnConnectedCallback;
        public Action OnDisconnectedCallback;
        public Action OnLeftCallback;

        public KarmanClient() {
            id = Guid.NewGuid();
            secret = Guid.NewGuid();
            SetupClient();
        }

        private void SetupClient() {
            client = new Client();
            client.OnConnectedCallback += OnConnected;
            client.OnDisconnectedCallback += OnDisconnected;
            client.OnPacketReceivedCallback += OnPacketReceived;
        }

        public void Start(string connectionString, int defaultPort) {
            Debug.Log(string.Format(
                "KarmanClient: Starting KarmanClient with connectionString={0} (defaultPort={1}), clientId={2}",
                connectionString, defaultPort, id
            ));
            endpoint = ConnectionString.Parse(connectionString, defaultPort);
            client.Start(endpoint);
        }

        private void OnConnected() {
            if (!hasJoined) {
                hasJoined = true;
                OnJoinedCallback();
            }
            reconnectionAttempt = false;
            OnConnectedCallback();
        }

        private void OnDisconnected() {
            OnDisconnectedCallback();
            if (reconnectionAttempt) {
                Debug.LogError("Client was unable to reconnect to the server, server will now be left");
                Leave();
                return;
            }
            if (!hasJoined) {
                Debug.LogError("Client was unable to connect to the server");
                return;
            }
            if (!left) {
                Debug.LogWarning("Connection with server lost, immediately trying to reconnect");
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
                Debug.Log(string.Format("Server says: {0}", messagePacket.GetMessage()));

            } else if (packet is ServerInformationPacket serverInformationPacket) {
                Debug.Log(string.Format(
                    "Server send its information serverId={0} and protocolVersion={1}",
                    serverInformationPacket.GetServerId(), serverInformationPacket.GetProtocolVersion()
                ));
                if (KarmanServer.PROTOCOL_VERSION != serverInformationPacket.GetProtocolVersion()) {
                    Debug.LogError(string.Format(
                        "Leaving server since it uses a different protocol version ({0}) than the client ({1}) is using",
                        KarmanServer.PROTOCOL_VERSION, serverInformationPacket.GetProtocolVersion()
                    ));
                    Leave();
                } else {
                    ClientInformationPacket provideUsernamePacket = new ClientInformationPacket(id, secret);
                    client.Send(provideUsernamePacket);
                }

            } else if (packet is LeavePacket) {
                Leave();

            } else {
                Debug.LogWarning(string.Format("KarmanClient: Did not handle a received packet that is of type {0}", packet.GetType().Name));
            }
        }

        public void Leave() {
            if (left) {
                Debug.LogWarning("Client cannot leave the server if it already left");
                return;
            }
            left = true;
            try {
                client.Send(new LeavePacket());
                System.Threading.Thread.Sleep(100); // TODO: fix this.
                client.Disconnect();
            } catch (Exception) {
                Debug.LogWarning(string.Format("Connection with server could not be disconnected"));
            }
            OnLeftCallback();
        }

        public void Send(Packet packet) {
            client.Send(packet);
        }
    }
}

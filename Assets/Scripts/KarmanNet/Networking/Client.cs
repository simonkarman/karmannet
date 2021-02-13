using KarmanNet.Logging;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace KarmanNet.Networking {
    public class Client : IConnection {
        private static readonly Logger log = Logger.For<Client>();
        public static readonly int MAX_FRAME_SIZE = 256;

        private readonly ManualResetEvent connectDone = new ManualResetEvent(false);
        private readonly ByteFramer byteFramer;
        private readonly ByteSender byteSender;
        private readonly Factory<Packet> packetFactory;

        public IPEndPoint serverEndpoint;
        private Socket socket;

        public ConnectionStatus Status { get; private set; } = ConnectionStatus.NEW;

        public Action OnConnectedCallback;
        public Action OnDisconnectedCallback;
        public Action<Packet> OnPacketReceivedCallback;

        public Client() {
            packetFactory = Factory<Packet>.BuildFromAllAssemblies();
            ThreadManager.Activate();
            byteFramer = new ByteFramer(MAX_FRAME_SIZE, OnFrameReceived);
            byteSender = new ByteSender(this);
        }

        public void Start(IPEndPoint serverEndpoint) {
            if (Status != ConnectionStatus.NEW) {
                throw log.ExitError(new ConnectionNotNewException("start"));
            }
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var connectingThread = new Thread(() => {
                log.Info("Starting client by setting up a connection to {0}", serverEndpoint);
                socket.BeginConnect(serverEndpoint, new AsyncCallback(ConnectCallback), null);

                connectDone.WaitOne();

                if (socket.Connected) {
                    log.Info("Succesfully connected to the server");
                    Status = ConnectionStatus.CONNECTED;
                    new ByteReceiver(this, OnBytesReceived);
                    OnConnected();
                } else {
                    log.Warning("Failed to connect to the server");
                    Status = ConnectionStatus.DISCONNECTED;
                    OnDisonnected();
                }
            });
            connectingThread.Start();
        }

        private void OnConnected() {
            ThreadManager.ExecuteOnMainThread(() => {
                OnConnectedCallback?.Invoke();
            });
        }

        private void OnDisonnected() {
            ThreadManager.ExecuteOnMainThread(() => {
                OnDisconnectedCallback?.Invoke();
            });
        }

        private void OnBytesReceived(byte[] bytes) {
            byteFramer.Append(bytes);
        }

        private void OnFrameReceived(byte[] bytes) {
            ThreadManager.ExecuteOnMainThread(() => {
                Packet packet = packetFactory.FromBytes(bytes);
                OnPacketReceivedCallback?.Invoke(packet);
            });
        }

        public Socket GetSocket() {
            return socket;
        }

        public string GetConnectedWithIdentifier() {
            return "server";
        }

        public bool IsConnected() {
            return Status == ConnectionStatus.CONNECTED;
        }

        private void ConnectCallback(IAsyncResult ar) {
            try {
                if (Status != ConnectionStatus.NEW) {
                    log.Error("Client cannot handle a connect callback when it is not new");
                    return;
                }
                socket.EndConnect(ar);
            } catch (Exception e) {
                log.Error("Client cannot handle a connect callback due to the following exception: {0}", e.ToString());
            } finally {
                connectDone.Set();
            }
        }

        public void Send(Packet packet) {
            byte[] bytes = packetFactory.GetBytes(packet);
            byte[] frame = byteFramer.Frame(bytes);
            byteSender.Send(frame);
        }

        public void Disconnect() {
            if (Status != ConnectionStatus.CONNECTED) {
                log.Warning("Client cannot disconnect when it is not connected");
                return;
            }
            if (!socket.Connected) {
                Status = ConnectionStatus.DISCONNECTED;
                return;
            }
            log.Info("Disconnecting from server");
            Status = ConnectionStatus.DISCONNECTED;
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            log.Info("Successfully disconnected from the server");
            OnDisonnected();
        }
    }
}

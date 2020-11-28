using Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Linq;

namespace Networking {
    public class Server {
        private static readonly Logger log = Logger.For<Server>();

        public class Connection : IConnection {
            public const int MAX_FRAME_SIZE = 256;

            private readonly Server server;
            public readonly Guid connectionId;
            private readonly Socket socket;
            private readonly ByteFramer byteFramer;
            private readonly ByteSender byteSender;

            public ConnectionStatus Status { get; private set; } = ConnectionStatus.CONNECTED;

            public Socket GetSocket() {
                return socket;
            }

            public string GetConnectedWithIdentifier() {
                return string.Format("connection-{0}", connectionId.ToString());
            }

            public bool IsConnected() {
                return Status == ConnectionStatus.CONNECTED;
            }

            public Connection(Server server, Guid connectionId, Socket socket) {
                this.server = server;
                this.connectionId = connectionId;
                this.socket = socket;

                byteFramer = new ByteFramer(MAX_FRAME_SIZE, (byte[] bytes) => {
                    server.OnFrameReceived(connectionId, bytes);
                });

                new ByteReceiver(this, (bytes) => {
                    byteFramer.Append(bytes);
                });

                byteSender = new ByteSender(this);
            }

            public void Disconnect() {
                if (Status == ConnectionStatus.DISCONNECTED) {
                    log.Error("Server cannot disconnect connection with connection {0} when it has already been disconnected", connectionId);
                    return;
                }
                log.Info("Disconnecting connection {0}", connectionId);
                Status = ConnectionStatus.DISCONNECTED;
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                server.OnDisconnected(connectionId);
                log.Info("Successfully disconnected connection {0}", connectionId);
            }

            public void Send(byte[] data) {
                byte[] frame = byteFramer.Frame(data);
                byteSender.Send(frame);
            }
        }

        private readonly ManualResetEvent acceptDone = new ManualResetEvent(false);
        private readonly Dictionary<Guid, Connection> connections = new Dictionary<Guid, Connection>();
        private readonly PacketFactory packetFactory;
        private Socket rootSocket;

        public ServerStatus Status { get; private set; } = ServerStatus.NEW;

        public Action OnRunningCallback;
        public Action OnShutdownCallback;
        public Action<Guid> OnConnectedCallback;
        public Action<Guid> OnDisconnectedCallback;
        public Action<Guid, Packet> OnPacketReceivedCallback;

        public Server() {
            packetFactory = PacketFactory.BuildFromAllAssemblies();
            ThreadManager.Activate();
        }

        public void Start(int port) {
            if (Status != ServerStatus.NEW) {
                throw log.ExitError(new Exception("Cannot start a server that is already running or has already been shutdown"));
            }
            log.Info(string.Format("Starting server on port {0}", port));
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, port);

            try {
                rootSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                rootSocket.Bind(localEndPoint);
                rootSocket.Listen(32);
            } catch (Exception err) {
                log.Error("Server could no start due to: {0}", err);
                OnShutdownCallback?.Invoke();
                return;
            }

            var thread = new Thread(() => {
                log.Info(string.Format("Server is ready for connections on {0}", localEndPoint));
                Status = ServerStatus.RUNNING;
                OnRunning();
                InitiateAcceptLoop();
            });
            thread.Start();
        }

        private void OnRunning() {
            ThreadManager.ExecuteOnMainThread(() => {
                OnRunningCallback?.Invoke();
            });
        }

        private void OnShutdown() {
            ThreadManager.ExecuteOnMainThread(() => {
                OnShutdownCallback?.Invoke();
            });
        }

        private void OnConnected(Guid connectionId) {
            ThreadManager.ExecuteOnMainThread(() => {
                OnConnectedCallback?.Invoke(connectionId);
            });
        }

        private void OnDisconnected(Guid connectionId) {
            connections.Remove(connectionId);
            ThreadManager.ExecuteOnMainThread(() => {
                OnDisconnectedCallback?.Invoke(connectionId);
            });
        }

        private void OnFrameReceived(Guid connectionId, byte[] bytes) {
            ThreadManager.ExecuteOnMainThread(() => {
                OnPacketReceivedCallback?.Invoke(connectionId, packetFactory.FromBytes(bytes));
            });
        }

        public void InitiateAcceptLoop() {
            while (true) {
                acceptDone.Reset();
                if (Status == ServerStatus.SHUTDOWN) {
                    log.Info("Breaking out of accepting new connections loop since server has shutdown");
                    break;
                }

                rootSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);

                acceptDone.WaitOne();
            }
        }

        public void AcceptCallback(IAsyncResult ar) {
            try {
                if (Status != ServerStatus.RUNNING) {
                    log.Info("Server cannot handle an accept callback when it is not running");
                    return;
                }

                Socket socket = rootSocket.EndAccept(ar);
                Guid connectionId = Guid.NewGuid();
                log.Info("Accepted incoming connection from {0} and assigned id {1} to the connection", socket.RemoteEndPoint, connectionId);
                Connection connection = new Connection(this, connectionId, socket);
                connections.Add(connectionId, connection);
                OnConnected(connectionId);

            } catch (Exception) {

            } finally {
                acceptDone.Set();
            }
        }

        public void Shutdown() {
            if (Status != ServerStatus.RUNNING) {
                log.Error("Server cannot shutdown when it is not running");
                return;
            }
            log.Info("Shutting down server");
            Status = ServerStatus.SHUTDOWN;
            var connectionIds = connections.Keys.ToList();
            foreach (var connectionId in connectionIds) {
                Connection connection = connections[connectionId];
                connection.Disconnect();
            }
            rootSocket.Close();
            log.Info("Server shutdown completed");
            OnShutdown();
        }

        public void Disconnect(Guid connectionId) {
            if (connections.TryGetValue(connectionId, out Connection connection)) {
                connection.Disconnect();
            } else {
                throw log.ExitError(new ConnectionNotFoundException(connectionId));
            }
        }

        public void Broadcast(Packet packet) {
            byte[] bytes = packetFactory.GetBytes(packet);
            foreach (var connection in connections.Values) {
                connection.Send(bytes);
            }
        }

        public void Send(Guid connectionId, Packet packet) {
            byte[] bytes = packetFactory.GetBytes(packet);
            if (connections.TryGetValue(connectionId, out Connection connection)) {
                connection.Send(bytes);
            } else {
                throw log.ExitError(new ConnectionNotFoundException(connectionId));
            }
        }
    }
}

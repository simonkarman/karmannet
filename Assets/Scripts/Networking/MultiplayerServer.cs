using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using System.Linq;


namespace Networking {
    public class MultiplayerServer {

        public class Connection {
            private readonly MultiplayerServer server;
            public readonly Guid connectionId;
            public readonly Socket socket;
            public readonly ByteFramer byteFramer;

            private readonly ManualResetEvent receiveDone = new ManualResetEvent(false);
            private byte[] receiveBuffer = new byte[RECEIVING_BUFFER_SIZE];

            public ConnectionStatus Status { get; private set; } = ConnectionStatus.CONNECTED;

            private float connectionEstablishedTimestamp;
            public float RealtimeSinceConnectionEstablished {
                get {
                    return Time.realtimeSinceStartup - connectionEstablishedTimestamp;
                }
            }

            public Connection(MultiplayerServer server, Guid connectionId, Socket socket) {
                this.server = server;
                this.connectionId = connectionId;
                this.socket = socket;

                byteFramer = new ByteFramer(MAX_FRAME_SIZE, (byte[] bytes) => {
                    server.OnFrameReceived(connectionId, bytes);
                });

                ThreadManager.ExecuteOnMainThread(() => {
                    connectionEstablishedTimestamp = Time.realtimeSinceStartup;
                });
                server.OnConnected(connectionId);

                new Thread(InitiateReceiveLoop).Start();
            }

            public void Disconnect() {
                if (Status == ConnectionStatus.DISCONNECTED) {
                    Debug.LogError(string.Format("Server cannot disconnect connection {0} when it has already been disconnected", connectionId));
                    return;
                }
                Debug.Log(string.Format("Disconnecting connection {0}", connectionId));
                Status = ConnectionStatus.DISCONNECTED;
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                server.OnDisconnected(connectionId);
                Debug.Log(string.Format("Successfully disconnected {0}", connectionId));
            }


            public void Send(byte[] bytes) {
                if (Status == ConnectionStatus.DISCONNECTED) {
                    Debug.LogError(string.Format("Connection {0} cannot send when it is disconnected", connectionId));
                    return;
                }

                byte[] frame = byteFramer.Frame(bytes);
                Debug.Log(string.Format(
                    "Sending a frame of {0} byte(s) to connection {1}: {2}{3}",
                    frame.Length,
                    connectionId,
                    BitConverter.ToString(frame, 0, Math.Min(16, frame.Length)),
                    frame.Length > 16 ? "-.." : string.Empty
                ));

                socket.BeginSend(frame, 0, frame.Length, 0, new AsyncCallback(SendCallback), null);
            }

            private void SendCallback(IAsyncResult ar) {
                if (Status == ConnectionStatus.DISCONNECTED) {
                    Debug.LogError(string.Format("Connection {0} cannot handle a send callback when it is disconnected", connectionId));
                    return;
                }

                try {
                    int bytesSent = socket.EndSend(ar);
                    Debug.Log(string.Format("Successfully sent {0} byte(s) to the connection {1}.", bytesSent, connectionId));
                } catch (Exception e) {
                    Debug.LogError(string.Format("An error occurred in the send callback of connection {0}: {1}", connectionId, e.ToString()));
                }
            }

            private void InitiateReceiveLoop() {
                Debug.Log(string.Format("Initiated receive loop, connection {0} is ready for incoming frames", connectionId));
                while (true) {
                    receiveDone.Reset();
                    if (Status != ConnectionStatus.CONNECTED) {
                        Debug.Log(string.Format("Breaking out of receive loop since connection {0} is no longer connected", connectionId));
                        break;
                    }

                    socket.BeginReceive(receiveBuffer, 0, receiveBuffer.Length, 0, new AsyncCallback(ReceiveCallback), null);
                    receiveDone.WaitOne();
                }
            }

            private void ReceiveCallback(IAsyncResult ar) {
                try {
                    int bytesRead = socket.Connected ? socket.EndReceive(ar) : 0;
                    if (bytesRead == 0) {
                        if (Status == ConnectionStatus.CONNECTED) {
                            Debug.Log(string.Format("Handling a receive callback containing 0 bytes or the socket is no longer connected, this means that connection {0} should be disconnected", connectionId));
                            Disconnect();
                        }
                        return;
                    }

                    Debug.Log(string.Format("Received {0} bytes from connection {1}.", bytesRead, connectionId));
                    byte[] bytes = new byte[bytesRead];
                    Buffer.BlockCopy(receiveBuffer, 0, bytes, 0, bytesRead);
                    byteFramer.Append(bytes);

                } catch (Exception e) {
                    Debug.LogError(string.Format("An error occurred in the receive callback of connection {0}: {1}", connectionId, e.ToString()));
                } finally {
                    receiveDone.Set();
                }
            }
        }

        public const int DEFAULT_PORT = 14641;
        public const int RECEIVING_BUFFER_SIZE = 256;
        public const int MAX_FRAME_SIZE = 256;

        private ManualResetEvent acceptDone = new ManualResetEvent(false);
        private readonly Socket rootSocket;
        private readonly Dictionary<Guid, Connection> connections = new Dictionary<Guid, Connection>();
        private readonly Action<Guid> OnConnected;
        private readonly Action<Guid> OnDisconnected;
        private readonly Action<Guid, byte[]> OnFrameReceived;

        public ServerStatus Status { get; private set; } = ServerStatus.RUNNING;

        private float startedTimestamp;
        public float RealtimeSinceStarted {
            get {
                return Time.realtimeSinceStartup - startedTimestamp;
            }
        }

        public MultiplayerServer(Action<Guid> OnConnected, Action<Guid> OnDisconnected, Action<Guid, byte[]> OnFrameReceived) :
            this(DEFAULT_PORT, OnConnected, OnDisconnected, OnFrameReceived) {
        }

        public MultiplayerServer(int port, Action<Guid> OnConnected, Action<Guid> OnDisconnected, Action<Guid, byte[]> OnFrameReceived) {
            Debug.Log(string.Format("Start of setting up server on port {0}", port));
            startedTimestamp = Time.realtimeSinceStartup;

            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, port);
            ThreadManager.Activate();

            this.OnConnected = (Guid connectionId) => {
                ThreadManager.ExecuteOnMainThread(() => {
                    OnConnected(connectionId);
                });
            };
            this.OnDisconnected = (Guid connectionId) => {
                connections.Remove(connectionId);
                ThreadManager.ExecuteOnMainThread(() => {
                    OnDisconnected(connectionId);
                });
            };
            this.OnFrameReceived = (Guid connectionId, byte[] frame) => {
                ThreadManager.ExecuteOnMainThread(() => {
                    OnFrameReceived(connectionId, frame);
                });
            };
            ;

            rootSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            rootSocket.Bind(localEndPoint);
            rootSocket.Listen(100);

            var thread = new Thread(() => {
                Debug.Log(string.Format("Server is ready for connections on {0}", localEndPoint));
                InitiateAcceptLoop();
            });
            thread.Start();
        }

        public void InitiateAcceptLoop() {
            while (true) {
                acceptDone.Reset();
                if (Status == ServerStatus.SHUTDOWN) {
                    Debug.Log("Breaking out of accepting new connections loop since server has shutdown");
                    break;
                }

                // TODO: When server is full, either stop accepting new connection or allow them and notify them instantly after connection was setup
                rootSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);

                acceptDone.WaitOne();
            }
        }

        public void AcceptCallback(IAsyncResult ar) {
            try {
                if (Status == ServerStatus.SHUTDOWN) {
                    Debug.Log("Server cannot handle an accept callback after it was shutdown");
                    return;
                }

                Socket socket = rootSocket.EndAccept(ar);
                Guid connectionId = Guid.NewGuid();
                Debug.Log(string.Format("Accepted incoming connection from {0} and assigned id {1} to the connection", socket.RemoteEndPoint, connectionId));
                Connection connection = new Connection(this, connectionId, socket);
                connections.Add(connectionId, connection);

            } catch (Exception) {

            } finally {
                acceptDone.Set();
            }
        }

        public void Shutdown() {
            if (Status == ServerStatus.SHUTDOWN) {
                Debug.LogError("Server cannot shutdown when it has already been shutdown");
                return;
            }
            Debug.Log("Shutting down server");
            Status = ServerStatus.SHUTDOWN;
            var connectionIds = connections.Keys.ToList();
            foreach (var connectionId in connectionIds) {
                Connection connection = connections[connectionId];
                connection.Disconnect();
            }
            rootSocket.Close();
            Debug.Log("Server shutdown completed");
        }

        public void Broadcast(byte[] frame) {
            foreach (var connection in connections.Values) {
                connection.Send(frame);
            }
        }

        public void Send(Guid connectionId, byte[] frame) {
            if (connections.TryGetValue(connectionId, out Connection connection)) {
                connection.Send(frame);
            } else {
                throw new InvalidOperationException(string.Format("Cannot send a message to connection {0} because that connection does not exist", connectionId));
            }
        }
    }
}

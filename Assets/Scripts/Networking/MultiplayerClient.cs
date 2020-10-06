using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace Networking {
    public class MultiplayerClient : IConnection {
        public static readonly int MAX_FRAME_SIZE = 256;

        public readonly IPEndPoint serverEndpoint;
        private readonly Socket socket;
        private readonly ManualResetEvent connectDone = new ManualResetEvent(false);
        private readonly ByteFramer byteFramer;
        private readonly ByteSender byteSender;
        private readonly PacketFactory packetFactory;

        private float connectionEstablishedTimestamp;
        public float RealtimeSinceConnectionEstablished {
            get {
                return Time.realtimeSinceStartup - connectionEstablishedTimestamp;
            }
        }
        public ConnectionStatus Status { get; private set; } = ConnectionStatus.NEW;

        public Socket GetSocket() {
            return socket;
        }

        public string GetConnectedWithIdentifier() {
            return "server";
        }

        public bool IsConnected() {
            return Status == ConnectionStatus.CONNECTED;
        }

        public MultiplayerClient(IPEndPoint serverEndpoint, PacketFactory packetFactory, Action<Packet> OnPacketReceived) {
            Debug.Log(string.Format("Start of setting up connection to {0}", serverEndpoint));
            Debug.Log(packetFactory.ToString());
            this.packetFactory = packetFactory;
            ThreadManager.Activate();
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            byteFramer = new ByteFramer(MAX_FRAME_SIZE, (byte[] bytes) => {
                ThreadManager.ExecuteOnMainThread(() => {
                    OnPacketReceived(packetFactory.FromBytes(bytes));
                });
            });
            byteSender = new ByteSender(this);

            var connectingThread = new Thread(() => {
                Debug.Log(string.Format("Connecting to server at {0}", serverEndpoint));
                socket.BeginConnect(serverEndpoint, new AsyncCallback(ConnectCallback), null);

                connectDone.WaitOne();

                if (socket.Connected) {
                    Debug.Log("Succesfully connected to the server");
                    Status = ConnectionStatus.CONNECTED;
                    ThreadManager.ExecuteOnMainThread(() => {
                        connectionEstablishedTimestamp = Time.realtimeSinceStartup;
                    });
                    new ByteReceiver(this, (bytes) => {
                        byteFramer.Append(bytes);
                    });
                } else {
                    Debug.LogWarning("Failed to connect to the server");
                    Status = ConnectionStatus.DISCONNECTED;
                }
            });
            connectingThread.Start();
        }

        private void ConnectCallback(IAsyncResult ar) {
            try {
                if (Status != ConnectionStatus.NEW) {
                    Debug.LogError("Client cannot handle a connect callback when it is not new");
                    return;
                }
                socket.EndConnect(ar);
            } catch (Exception e) {
                Debug.LogError(e.ToString());
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
                Debug.LogError("Client cannot disconnect when it is not connected");
                return;
            }
            if (!socket.Connected) {
                Status = ConnectionStatus.DISCONNECTED;
                return;
            }
            Debug.Log("Disconnecting from server");
            Status = ConnectionStatus.DISCONNECTED;
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            Debug.Log("Successfully disconnected from the server");
        }
    }
}

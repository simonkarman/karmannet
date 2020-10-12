using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace Networking {
    public class Client : IConnection {
        public static readonly int MAX_FRAME_SIZE = 256;

        private readonly ManualResetEvent connectDone = new ManualResetEvent(false);
        private readonly ByteFramer byteFramer;
        private readonly ByteSender byteSender;
        private readonly PacketFactory packetFactory;

        public IPEndPoint serverEndpoint;
        private Socket socket;

        public ConnectionStatus Status { get; private set; } = ConnectionStatus.NEW;

        public Action OnConnectedCallback;
        public Action OnDisconnectedCallback;
        public Action<Packet> OnPacketReceivedCallback;

        public Client() {
            packetFactory = PacketFactory.BuildFromAllAssemblies();
            Debug.Log(packetFactory.ToString());
            ThreadManager.Activate();
            byteFramer = new ByteFramer(MAX_FRAME_SIZE, OnFrameReceived);
            byteSender = new ByteSender(this);
        }

        public void Start(IPEndPoint serverEndpoint) {
            if (Status != ConnectionStatus.NEW) {
                throw new InvalidOperationException("Cannot start a client that is already connected or has already left");
            }
            Debug.Log(string.Format("Start of setting up connection to {0}", serverEndpoint));
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var connectingThread = new Thread(() => {
                Debug.Log(string.Format("Connecting to server at {0}", serverEndpoint));
                socket.BeginConnect(serverEndpoint, new AsyncCallback(ConnectCallback), null);

                connectDone.WaitOne();

                if (socket.Connected) {
                    Debug.Log("Succesfully connected to the server");
                    Status = ConnectionStatus.CONNECTED;
                    new ByteReceiver(this, OnBytesReceived);
                    OnConnected();
                } else {
                    Debug.LogWarning("Failed to connect to the server");
                    Status = ConnectionStatus.DISCONNECTED;
                    OnDisonnected();
                }
            });
            connectingThread.Start();
        }

        private void OnConnected() {
            ThreadManager.ExecuteOnMainThread(() => {
                OnConnectedCallback();
            });
        }

        private void OnDisonnected() {
            ThreadManager.ExecuteOnMainThread(() => {
                OnDisconnectedCallback();
            });
        }

        private void OnBytesReceived(byte[] bytes) {
            byteFramer.Append(bytes);
        }

        private void OnFrameReceived(byte[] bytes) {
            ThreadManager.ExecuteOnMainThread(() => {
                Packet packet = packetFactory.FromBytes(bytes);
                OnPacketReceivedCallback(packet);
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
            OnDisonnected();
            Debug.Log("Successfully disconnected from the server");
        }
    }
}

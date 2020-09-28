using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace Networking {
    public class MultiplayerClient : IConnection {
        public static readonly int RECEIVING_BUFFER_SIZE = 256;
        public static readonly int MAX_FRAME_SIZE = 256;

        public readonly IPEndPoint serverEndpoint;
        private readonly Socket socket;
        private readonly ManualResetEvent connectDone = new ManualResetEvent(false);
        private readonly ManualResetEvent receiveDone = new ManualResetEvent(false);
        private readonly ByteFramer byteFramer;
        private readonly ByteSender byteSender;
        private byte[] receiveBuffer = new byte[RECEIVING_BUFFER_SIZE];

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

        public string GetIdentifier() {
            return "Connection from Client to Server";
        }

        public bool IsConnected() {
            return Status == ConnectionStatus.CONNECTED;
        }

        public MultiplayerClient(IPEndPoint serverEndpoint, Action<byte[]> OnFrameReceived) {
            Debug.Log(string.Format("Start of setting up connection to {0}", serverEndpoint));
            ThreadManager.Activate();
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            byteFramer = new ByteFramer(MAX_FRAME_SIZE, OnFrameReceived);
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
                    InitiateReceiveLoop();
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

        public void Send(byte[] data) {
            byte[] frame = byteFramer.Frame(data);
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

        private void InitiateReceiveLoop() {
            Debug.Log("Initiated receive loop, client is ready for incoming frames from the server");
            while (true) {
                receiveDone.Reset();
                if (Status != ConnectionStatus.CONNECTED) {
                    Debug.Log("Breaking out of receive loop since client is no longer connected");
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
                        Debug.Log("Handling a receive callback containing 0 bytes or the socket is no longer connected, this means the connection should be disconnected");
                        Disconnect();
                    }
                    return;
                }

                Debug.Log(string.Format("Received {0} bytes from the server.", bytesRead));
                byte[] bytes = new byte[bytesRead];
                Buffer.BlockCopy(receiveBuffer, 0, bytes, 0, bytesRead);
                byteFramer.Append(bytes);

            } catch (Exception e) {
                Debug.LogError(string.Format("An error occurred in the receive callback: {0}", e.ToString()));
            } finally {
                receiveDone.Set();
            }
        }
    }
}

using System;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace Networking {
    public class ByteReceiver {
        public const int RECEIVING_BUFFER_SIZE = 256;

        private readonly IConnection connection;
        private readonly Action<byte[]> OnBytesReceived;

        private readonly ManualResetEvent receiveDone = new ManualResetEvent(false);
        private readonly byte[] receiveBuffer = new byte[RECEIVING_BUFFER_SIZE];

        public ByteReceiver(IConnection connection, Action<byte[]> OnBytesReceived) {
            this.connection = connection;
            this.OnBytesReceived = OnBytesReceived;

            new Thread(() => InitiateReceiveLoop()).Start();
        }

        private void InitiateReceiveLoop() {
            Debug.Log(string.Format("Ready for incoming frames from {0}", connection.GetConnectedWithIdentifier()));
            while (true) {
                receiveDone.Reset();
                if (!connection.IsConnected()) {
                    Debug.Log(string.Format("Breaking out of receive loop since connection with {0} is no longer alive", connection.GetConnectedWithIdentifier()));
                    break;
                }

                connection.GetSocket().BeginReceive(receiveBuffer, 0, receiveBuffer.Length, 0, new AsyncCallback(ReceiveCallback), null);
                receiveDone.WaitOne();
            }
        }

        private void ReceiveCallback(IAsyncResult ar) {
            try {
                Socket socket = connection.GetSocket();
                int bytesRead = socket.Connected ? socket.EndReceive(ar) : 0;
                if (bytesRead == 0) {
                    if (connection.IsConnected()) {
                        Debug.Log(string.Format("Handling a receive callback containing 0 bytes or the socket is no longer connected, this means that connection with {0} should be disconnected", connection.GetConnectedWithIdentifier()));
                        connection.Disconnect();
                    }
                    return;
                }

                Debug.Log(string.Format("Received {0} bytes from {1}", bytesRead, connection.GetConnectedWithIdentifier()));
                byte[] bytes = new byte[bytesRead];
                Buffer.BlockCopy(receiveBuffer, 0, bytes, 0, bytesRead);
                OnBytesReceived(bytes);

            } catch (Exception e) {
                Debug.LogError(string.Format("An error occurred in the receive callback from {0}: {1}", connection.GetConnectedWithIdentifier(), e.ToString()));
            } finally {
                receiveDone.Set();
            }
        }
    }
}

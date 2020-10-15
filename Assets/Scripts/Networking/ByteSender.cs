using System;
using UnityEngine;

namespace Networking {
    public class ByteSender {
        private readonly IConnection connection;

        public ByteSender(IConnection connection) {
            this.connection = connection;
        }

        public void Send(byte[] frame) {
            if (!connection.IsConnected()) {
                throw new InvalidOperationException(string.Format("Cannot sent frame to {0} when it is not connected", connection.GetConnectedWithIdentifier()));
            }
            /*if (!connection.GetSocket().Connected) {
                connection.Disconnect();
                return;
            }*/

            /*Debug.Log(string.Format(
                "Sending a frame of {0} byte(s) to {1}: {2}{3}",
                frame.Length,
                connection.GetConnectedWithIdentifier(),
                BitConverter.ToString(frame, 0, Math.Min(16, frame.Length)),
                frame.Length > 16 ? "-.." : string.Empty
            ));*/
            connection.GetSocket().BeginSend(frame, 0, frame.Length, 0, new AsyncCallback(SendCallback), null);
        }

        private void SendCallback(IAsyncResult ar) {
            if (!connection.IsConnected()) {
                Debug.LogError(string.Format("Cannot handle a send callback from {0} when it is not connected", connection.GetConnectedWithIdentifier()));
                return;
            }

            try {
                int bytesSent = connection.GetSocket().EndSend(ar);
            } catch (Exception e) {
                Debug.LogError(string.Format("An error occurred in the send callback from {0}: {1}", connection.GetConnectedWithIdentifier(), e.ToString()));
            }
        }
    }
}

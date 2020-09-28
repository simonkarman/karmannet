using System;
using UnityEngine;

namespace Networking {
    public class ByteSender {
        public static bool DEBUG_LOGGING = false;
        private readonly IConnection connection;

        public ByteSender(IConnection connection) {
            this.connection = connection;
        }

        public void Send(byte[] frame) {
            if (!connection.IsConnected()) {
                throw new InvalidOperationException(string.Format("Connection with {0} cannot sent when it is not connected", connection.GetIdentifier()));
            }

            Debug.Log(string.Format(
                "Connection with {0} is sending a frame of {1} byte(s): {2}{3}",
                connection.GetIdentifier(),
                frame.Length,
                BitConverter.ToString(frame, 0, Math.Min(16, frame.Length)),
                frame.Length > 16 ? "-.." : string.Empty
            ));
            connection.GetSocket().BeginSend(frame, 0, frame.Length, 0, new AsyncCallback(SendCallback), null);
        }

        private void SendCallback(IAsyncResult ar) {
            if (!connection.IsConnected()) {
                Debug.LogError(string.Format("Connection with {0} cannot handle a send callback when it is not connected", connection.GetIdentifier()));
                return;
            }

            int bytesSent = connection.GetSocket().EndSend(ar);
            Debug.Log(string.Format("Connection with {0} successfully sent {1} byte(s).", connection.GetIdentifier(), bytesSent));
        }
    }
}

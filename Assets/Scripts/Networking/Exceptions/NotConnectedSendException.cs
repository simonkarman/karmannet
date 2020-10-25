using System;

namespace Networking {
    public class NotConnectedSendException : Exception {
        public NotConnectedSendException(string connectedWith):
            base(string.Format("Cannot sent frame to {0} when it is not connected", connectedWith)) {
        }
    }
}
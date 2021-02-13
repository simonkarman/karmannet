namespace KarmanNet.Networking {
    public class NotConnectedSendException : NetworkingException {
        public NotConnectedSendException(string connectedWith):
            base(string.Format("Cannot sent frame to {0} when it is not connected", connectedWith)) {
        }
    }
}
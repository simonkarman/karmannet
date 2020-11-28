namespace Networking {
    public class ConnectionNotNewException : NetworkingException {
        public ConnectionNotNewException(string operation) :
            base(string.Format("Cannot perform '{0}' on a connection that is already connected or disconnected", operation)) {
        }
    }
}
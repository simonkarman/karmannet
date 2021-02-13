using System;

namespace KarmanNet.Networking {
    public class ConnectionNotFoundException : NetworkingException {
        public ConnectionNotFoundException(Guid connectionId) :
            base(string.Format("Connection {0} does not exist", connectionId)) {
        }
    }
}
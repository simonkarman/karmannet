using System;

namespace Networking {
    public class ConnectionNotFoundException : NetworkingException {
        public ConnectionNotFoundException(Guid connectionId) :
            base(string.Format("Connection {0} does not exist", connectionId)) {
        }
    }
}
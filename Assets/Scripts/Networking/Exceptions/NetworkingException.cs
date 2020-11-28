using System;

namespace Networking {
    public class NetworkingException : Exception {
        public NetworkingException(string message) : base(message) { }
    }
}
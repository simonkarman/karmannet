using System;

namespace KarmanNet.Networking {
    public class NetworkingException : Exception {
        public NetworkingException(string message) : base(message) { }
    }
}
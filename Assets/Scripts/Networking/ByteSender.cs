using System.Net.Sockets;

namespace Networking {
    public interface IConnection {
        Socket GetSocket();
        string GetIdentifier();
        bool IsConnected();
    }

    public class ByteSender {

        public IConnection connection;

        public ByteSender(IConnection connection) {

        }

    }
}

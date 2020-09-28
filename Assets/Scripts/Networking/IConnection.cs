using System.Net.Sockets;

namespace Networking {
    public interface IConnection {
        Socket GetSocket();
        string GetConnectedWithIdentifier();
        bool IsConnected();
        void Disconnect();
    }
}

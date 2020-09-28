using System.Net.Sockets;

namespace Networking {
    public interface IConnection {
        Socket GetSocket();
        string GetIdentifier();
        bool IsConnected();
    }
}

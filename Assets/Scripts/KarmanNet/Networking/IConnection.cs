using System.Net.Sockets;

namespace KarmanNet.Networking {
    public interface IConnection {
        Socket GetSocket();
        string GetConnectedWithIdentifier();
        bool IsConnected();
        void Disconnect();
    }
}

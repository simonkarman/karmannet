using System;
using System.Net;

namespace Networking {
    public static class ConnectionString {
        public static IPEndPoint Parse(string connectionString, int defaultPort) {
            try {
                string[] parts = connectionString.Split(':');
                if (parts[0] == "localhost") {
                    parts[0] = "127.0.0.1";
                }
                if (!IPAddress.TryParse(parts[0], out IPAddress ipAddress)) {
                    ipAddress = Dns.GetHostEntry(parts[0]).AddressList[0];
                }
                int port = parts.Length == 1 ? defaultPort : int.Parse(parts[1]);
                return new IPEndPoint(ipAddress, port);
            } catch (Exception) {
                throw new InvalidOperationException("Invalid connection string provided");
            }
        }
    }
}

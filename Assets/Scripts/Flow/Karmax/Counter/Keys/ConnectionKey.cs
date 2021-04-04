using KarmanNet.Karmax;
using KarmanNet.Networking;
using System;

namespace KarmaxCounter {
    public class ConnectionKey : FragmentKey {
        private readonly Guid connectionId;

        public ConnectionKey(byte[] bytes) : base(bytes) {
            connectionId = ReadGuid();
        }

        public ConnectionKey(Guid connectionId) : base(Bytes.Of(connectionId)) {
            this.connectionId = connectionId;
        }

        public override bool IsValid() {
            return true;
        }

        public Guid GetConnectionId() {
            return connectionId;
        }

        public override string AsString() {
            return $"connection/{connectionId}";
        }
    }
}
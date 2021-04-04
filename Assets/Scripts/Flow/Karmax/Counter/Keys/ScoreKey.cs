using KarmanNet.Karmax;
using KarmanNet.Networking;
using System;

namespace KarmaxCounter {
    public class ScoreKey : FragmentKey {
        private readonly Guid connectionId;

        public ScoreKey(byte[] bytes) : base(bytes) {
            connectionId = ReadGuid();
        }

        public ScoreKey(Guid connectionId) : base(Bytes.Of(connectionId)) {
            this.connectionId = connectionId;
        }

        public override bool IsValid() {
            return true;
        }

        public Guid GetConnectionId() {
            return connectionId;
        }

        public override string AsString() {
            return $"score/{connectionId}";
        }
    }
}
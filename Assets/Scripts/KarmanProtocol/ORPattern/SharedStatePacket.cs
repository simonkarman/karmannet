using Networking;
using System;

namespace KarmanProtocol.ORPattern {
    public abstract class SharedStatePacket<ImmutableT> : Packet {
        private readonly Guid requestId;

        public SharedStatePacket(byte[] bytes) : base(bytes) {
            requestId = ReadGuid();
        }

        public SharedStatePacket(Guid requestId, byte[] bytes) : base(Bytes.Pack(Bytes.Of(requestId), bytes)) {
            this.requestId = requestId;
        }

        public string GetSharedStateIdentifier() {
            return typeof(ImmutableT).FullName;
        }

        public Guid GetRequestId() {
            return requestId;
        }
    }
}

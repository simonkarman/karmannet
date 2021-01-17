using System;

namespace KarmanProtocol.ORPattern {
    public abstract class ChangeStateRequest<ImmutableT> : SharedStatePacket<ImmutableT> {
        public ChangeStateRequest(byte[] bytes) : base(bytes) { }
        public ChangeStateRequest(Guid requestId, byte[] bytes) : base(requestId, bytes) { }
    }
}

using System;

namespace KarmanProtocol.ORPattern {
    public abstract class StateChangedEvent<ImmutableT> : SharedStatePacket<ImmutableT> {
        public StateChangedEvent(byte[] bytes) : base(bytes) { }
        public StateChangedEvent(Guid requestId, byte[] bytes) : base(requestId, bytes) { }
    }
}

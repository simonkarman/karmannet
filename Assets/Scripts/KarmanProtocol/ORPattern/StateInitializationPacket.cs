using System;

namespace KarmanProtocol.ORPattern {
    public abstract class StateInitializationPacket<MutableT, ImmutableT> : SharedStatePacket<ImmutableT> {
        public StateInitializationPacket(byte[] bytes) : base(bytes) { }
        public StateInitializationPacket(Guid requestId, byte[] bytes) : base(requestId, bytes) { }
        public abstract MutableT ToState();
    }
}

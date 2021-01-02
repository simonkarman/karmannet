using Networking;

namespace KarmanProtocol.ORPattern {
    public abstract class StateChangeEvent : Packet, IStateChangePacket {
        public StateChangeEvent(byte[] bytes) : base(bytes) { }
        public abstract string GetSharedStateIdentifier();
    }
}

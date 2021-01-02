using Networking;

namespace KarmanProtocol.ORPattern {
    public abstract class StateChangeRequest : Packet, IStateChangePacket {
        public StateChangeRequest(byte[] bytes) : base(bytes) { }
        public abstract string GetSharedStateIdentifier();
    }
}

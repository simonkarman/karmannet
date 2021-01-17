using Networking;

namespace KarmanProtocol.Karmax {
    public abstract class Fragment : ByteConstructable {
        protected Fragment(byte[] bytes) : base(bytes) { }
    }
}
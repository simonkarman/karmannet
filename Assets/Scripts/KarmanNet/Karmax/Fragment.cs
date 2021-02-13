using KarmanNet.Networking;

namespace KarmanNet.Karmax {
    public abstract class Fragment : ByteConstructable {
        protected Fragment(byte[] bytes) : base(bytes) { }
    }
}
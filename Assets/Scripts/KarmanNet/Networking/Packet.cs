namespace KarmanNet.Networking {
    public abstract class Packet : ByteConstructable {
        public Packet(byte[] bytes) : base(bytes) {}
    }
}

namespace Networking {
    public class PacketFactoryBytesException : NetworkingException {
        public PacketFactoryBytesException(string reason):
            base(reason) {
        }
    }
}
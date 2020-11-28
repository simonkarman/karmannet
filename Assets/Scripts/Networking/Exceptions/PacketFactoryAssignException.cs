namespace Networking {
    public class PacketFactoryAssignException : NetworkingException {
        public PacketFactoryAssignException(string reason):
            base(reason) {
        }
    }
}
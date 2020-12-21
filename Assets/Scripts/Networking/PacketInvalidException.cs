namespace Networking {
    public class PacketInvalidException : NetworkingException {
        public PacketInvalidException(string action, string packetName) :
            base(string.Format("{0} a(n) {1} that is invalid", action, packetName)) {
        }
    }
}
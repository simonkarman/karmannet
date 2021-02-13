namespace KarmanNet.Networking {
    public class ByteConstructableInstanceInvalidException : NetworkingException {
        public ByteConstructableInstanceInvalidException(string action, string packetName) :
            base(string.Format("Cannot {0} a(n) {1} that is invalid", action, packetName)) {
        }
    }
}
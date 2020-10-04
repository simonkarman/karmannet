namespace Networking {
    public abstract class Packet {
        private byte[] bytes;
        protected Packet(byte[] bytes) {
            this.bytes = bytes;
        }

        public byte[] GetBytesInternal() {
            return bytes;
        }
    }
}

using System;

namespace KarmanProtocol {
    public class ClientInformationPacket : Networking.Packet {
        private readonly Guid clientId;

        public ClientInformationPacket(byte[] bytes) : base(bytes) {
            clientId = new Guid(bytes);
        }

        public ClientInformationPacket(Guid clientId) : base(clientId.ToByteArray()) {
            this.clientId = clientId;
        }

        public override void Validate() {
            if (clientId == Guid.Empty) {
                throw new InvalidOperationException("Cannot create a ClientInformationPacket with a clientId of Guid.Empty");
            }
        }

        public Guid GetClientId() {
            return clientId;
        }
    }
}

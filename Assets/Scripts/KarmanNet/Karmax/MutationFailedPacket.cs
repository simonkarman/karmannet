using KarmanNet.Networking;
using System;

namespace KarmanNet.Karmax {
    public class MutationFailedPacket : Packet {
        private readonly Guid id;
        private readonly string failureReason;

        public MutationFailedPacket(byte[] bytes) : base(bytes) {
            id = ReadGuid();
            failureReason = ReadString();
        }

        public MutationFailedPacket(Guid id, string failureReason) : base(
            Bytes.Pack(Bytes.Of(id), Bytes.Of(failureReason))
        ) {
            this.id = id;
            this.failureReason = failureReason;
        }

        public override bool IsValid() {
            return id != Guid.Empty && failureReason != null && failureReason.Length > 0;
        }
    }
}
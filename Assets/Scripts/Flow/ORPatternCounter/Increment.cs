using KarmanProtocol.ORPattern;
using System;
using Networking;

namespace ORPatternExample {
    public class Increment : ChangeStateRequest<Counter> {
        private readonly int amount;

        public Increment(byte[] bytes) : base(bytes) {
            amount = ReadInt();
        }

        public Increment(Guid requestId, int amount) : base(requestId, Bytes.Of(amount)) {
            this.amount = amount;
        }

        public override bool IsValid() {
            return true;
        }

        public int GetAmount() {
            return amount;
        }
    }
}
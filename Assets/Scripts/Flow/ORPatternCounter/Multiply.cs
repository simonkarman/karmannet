using KarmanProtocol.ORPattern;
using System;
using Networking;

namespace ORPatternExample {
    public class Multiply : ChangeStateRequest<Counter> {
        private readonly int product;

        public Multiply(byte[] bytes) : base(bytes) {
            product = ReadInt();
        }

        public Multiply(Guid requestId, int product) : base(requestId, Bytes.Of(product)) {
            this.product = product;
        }

        public override bool IsValid() {
            return true;
        }

        public int GetProduct() {
            return product;
        }
    }
}

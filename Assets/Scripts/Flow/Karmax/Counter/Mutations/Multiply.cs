using KarmanNet.Karmax;
using KarmanNet.Networking;
using System;

namespace KarmaxCounter {
    public class Multiply : Mutation.Update<Counter> {
        private readonly int product;

        public Multiply(byte[] bytes) : base(bytes) {
            product = Bytes.GetInt32(bytes);
        }

        private Multiply(int product) : base(Bytes.Of(product)) {
            this.product = product;
        }

        public static Multiply By(int product) {
            return new Multiply(product);
        }

        public override bool IsValid() {
            return true;
        }

        protected override Counter Mutate(Counter counterFragment, Guid requester) {
            return new Counter(counterFragment.GetValue() * product);
        }
    }
}

using KarmanNet.Karmax;
using KarmanNet.Networking;

namespace KarmaxCounter {
    public class Multiply : Mutation<CounterFragment> {
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

        public int GetProduct() {
            return product;
        }

        public override bool IsValid() {
            return true;
        }

        protected override CounterFragment Mutate(CounterFragment counterFragment) {
            return new CounterFragment(counterFragment.GetValue() * product);
        }
    }
}

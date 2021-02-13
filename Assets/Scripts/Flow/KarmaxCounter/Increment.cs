using KarmanNet.Karmax;
using KarmanNet.Networking;

namespace KarmaxCounter {
    public class Increment : Mutation<CounterFragment> {
        private readonly int amount;

        public Increment(byte[] bytes) : base(bytes) {
            amount = Bytes.GetInt32(bytes);
        }

        private Increment(int amount) : base(Bytes.Of(amount)) {
            this.amount = amount;
        }

        public static Increment By(int amount) {
            return new Increment(amount);
        }

        public int GetAmount() {
            return amount;
        }

        public override bool IsValid() {
            return true;
        }

        protected override CounterFragment Mutate(CounterFragment counterFragment) {
            return new CounterFragment(counterFragment.GetValue() + amount);
        }
    }
}

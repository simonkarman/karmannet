using KarmanNet.Karmax;
using KarmanNet.Networking;
using System;
using System.Collections.Generic;

namespace KarmaxCounter {
    public class Multiply : UpdateMutation<Counter> {
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

        protected override UpdateResult<Counter> Update(Counter fragment, IReadOnlyDictionary<FragmentKey, Fragment> state, FragmentKey key, Guid requester) {
            return UpdateResult<Counter>.Success(new Counter(fragment.GetValue() * product));
        }
    }
}

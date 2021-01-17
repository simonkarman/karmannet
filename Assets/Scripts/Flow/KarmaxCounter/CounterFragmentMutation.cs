using KarmanProtocol.Karmax;
using System;
using System.Collections.Generic;

namespace KarmaxExample {
    public abstract class CounterFragmentMutation : Mutation {
        public CounterFragmentMutation(byte[] bytes) : base(bytes) { }

        public override sealed MutationResult Mutate(IReadOnlyDictionary<string, Fragment> state, string fragmentId, Guid requester) {
            if (!state.GetFragmentOrIdentity(fragmentId, () => CounterFragment.Identity(), out CounterFragment counterFragment)) {
                return MutationResult.Failure($"{GetType().Name} (mutation) cannot mutate {state[fragmentId].GetType().Name} (fragment)");
            }
            return MutationResult.Ok(ApplyOn(counterFragment));
        }
        public abstract CounterFragment ApplyOn(CounterFragment counterFragment);
    }
}

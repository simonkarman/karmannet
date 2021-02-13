using Networking;
using System;
using System.Collections.Generic;

namespace KarmanProtocol.Karmax {
    public abstract class Mutation : ByteConstructable {
        protected Mutation(byte[] bytes) : base(bytes) { }
        public abstract MutationResult Mutate(IReadOnlyDictionary<string, Fragment> state, string fragmentId, Guid requester);
    }

    public abstract class Mutation<T> : Mutation where T : Fragment {
        public Mutation(byte[] bytes) : base(bytes) { }

        private T Identity() {
            return (T)typeof(T).GetMethod("Identity").Invoke(null, null);
        }

        public override sealed MutationResult Mutate(IReadOnlyDictionary<string, Fragment> state, string fragmentId, Guid requester) {
            if (!state.GetFragmentOrIdentity(fragmentId, Identity, out T fragment)) {
                return MutationResult.Failure($"{GetType().Name} (mutation) cannot mutate {state[fragmentId].GetType().Name} (fragment)");
            }
            return MutationResult.Success(Mutate(fragment));
        }
        protected abstract T Mutate(T fragment);
    }
}
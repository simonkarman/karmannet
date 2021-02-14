using System;
using System.Collections.Generic;

namespace KarmanNet.Karmax {
    public abstract class UpsertMutation<T> : Mutation where T : Fragment {
        protected UpsertMutation(byte[] bytes) : base(bytes) { }

        internal override sealed MutationResult Mutate(IReadOnlyDictionary<string, Fragment> state, string fragmentId, Guid requester) {
            MutationResult result;
            if (state.TryGetValue(fragmentId, out Fragment fragment)) {
                if (!(fragment is T fragmentT)) {
                    log.Warning($"Can not {GetType().Name} a {typeof(T).Name} at fragment[{fragmentId}], because that fragment is of type {fragment.GetType().Name}.");
                    return MutationResult.FragmentTypeMismatchFailure;
                }
                result = Update(fragmentT, state, fragmentId, requester);
                if (result == null) {
                    log.Warning($"{GetType().Name}.Update returned null, which is not allowed.");
                    return MutationResult.ResultNullFailure;
                }
            } else {
                result = Insert(state, fragmentId, requester);
                if (result == null) {
                    log.Warning($"{GetType().Name}.Insert returned null, which is not allowed.");
                    return MutationResult.ResultNullFailure;
                }
            }
            return result;
        }
        protected abstract InsertResult<T> Insert(IReadOnlyDictionary<string, Fragment> state, string fragmentId, Guid requester);
        protected abstract UpdateResult<T> Update(T fragment, IReadOnlyDictionary<string, Fragment> state, string fragmentId, Guid requester);
    }
}
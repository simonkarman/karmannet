using System;
using System.Collections.Generic;

namespace KarmanNet.Karmax {
    public abstract class UpsertMutation<T> : Mutation where T : Fragment {
        protected UpsertMutation(byte[] bytes) : base(bytes) { }

        internal override sealed MutationResult Mutate(IReadOnlyDictionary<FragmentKey, Fragment> state, FragmentKey key, Guid requester) {
            MutationResult result;
            if (state.TryGetValue(key, out Fragment fragment)) {
                if (!(fragment is T fragmentT)) {
                    log.Warning($"Can not {GetType().Name} a {typeof(T).Name} at fragment[{key.AsString()}], because that fragment is of type {fragment.GetType().Name}.");
                    return MutationResult.FragmentTypeMismatchFailure;
                }
                result = Update(fragmentT, state, key, requester);
                if (result == null) {
                    log.Warning($"{GetType().Name}.Update returned null, which is not allowed.");
                    return MutationResult.ResultNullFailure;
                }
            } else {
                result = Insert(state, key, requester);
                if (result == null) {
                    log.Warning($"{GetType().Name}.Insert returned null, which is not allowed.");
                    return MutationResult.ResultNullFailure;
                }
            }
            return result;
        }
        protected abstract InsertResult<T> Insert(IReadOnlyDictionary<FragmentKey, Fragment> state, FragmentKey key, Guid requester);
        protected abstract UpdateResult<T> Update(T fragment, IReadOnlyDictionary<FragmentKey, Fragment> state, FragmentKey key, Guid requester);
    }
}
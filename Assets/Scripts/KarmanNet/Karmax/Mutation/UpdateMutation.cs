using System;
using System.Collections.Generic;

namespace KarmanNet.Karmax {
    public abstract class UpdateMutation<T> : Mutation where T : Fragment {
        protected UpdateMutation(byte[] bytes) : base(bytes) { }

        internal override sealed MutationResult Mutate(IReadOnlyDictionary<FragmentKey, Fragment> state, FragmentKey key, Guid requester) {
            if (!state.TryGetValue(key, out Fragment fragment)) {
                log.Warning($"Can not {GetType().Name} a {typeof(T).Name} at fragment[{key.AsString()}], because that fragment does not exist.");
                return MutationResult.FragmentNotFoundFailure;
            }
            if (!(fragment is T fragmentT)) {
                log.Warning($"Can not {GetType().Name} a {typeof(T).Name} at fragment[{key.AsString()}], because that fragment is of type {fragment.GetType().Name}.");
                return MutationResult.FragmentTypeMismatchFailure;
            }
            MutationResult result = Update(fragmentT, state, key, requester);
            if (result == null) {
                log.Warning($"{GetType().Name}.Update returned null, while this is not allowed.");
                return MutationResult.ResultNullFailure;
            }
            return result;
        }
        protected abstract UpdateResult<T> Update(T fragment, IReadOnlyDictionary<FragmentKey, Fragment> state, FragmentKey fragmentId, Guid requester);
    }
}
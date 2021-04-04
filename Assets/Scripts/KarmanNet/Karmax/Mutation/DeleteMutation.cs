using System;
using System.Collections.Generic;

namespace KarmanNet.Karmax {
    public abstract class DeleteMutation<T> : Mutation where T : Fragment {
        protected DeleteMutation(byte[] bytes) : base(bytes) { }

        internal override sealed MutationResult Mutate(IReadOnlyDictionary<FragmentKey, Fragment> state, FragmentKey key, Guid requester) {
            MutationResult result;
            if (!state.TryGetValue(key, out Fragment fragment)) {
                log.Warning($"Can not {GetType().Name} a {typeof(T).Name} at fragment[{key.AsString()}], because that fragment does not exist.");
                return MutationResult.FragmentNotFoundFailure;
            }
            if (!(fragment is T fragmentT)) {
                log.Warning($"Can not {GetType().Name} a {typeof(T).Name} at fragment[{key.AsString()}], because that fragment is of type {fragment.GetType().Name}.");
                return MutationResult.FragmentTypeMismatchFailure;
            }
            result = Delete(fragmentT, state, key, requester);
            if (result == null) {
                log.Warning($"{GetType().Name}.Delete returned null, which is not allowed.");
                return MutationResult.ResultNullFailure;
            }
            return result;
        }
        protected abstract DeleteResult Delete(T fragment, IReadOnlyDictionary<FragmentKey, Fragment> state, FragmentKey fragmentId, Guid requester);
    }
}
using System;
using System.Collections.Generic;

namespace KarmanNet.Karmax {
    public abstract class InsertMutation<T> : Mutation where T : Fragment {
        protected InsertMutation(byte[] bytes) : base(bytes) { }

        internal override MutationResult Mutate(IReadOnlyDictionary<FragmentKey, Fragment> state, FragmentKey key, Guid requester) {
            if (state.ContainsKey(key)) {
                log.Warning($"Can not {GetType().Name} a {typeof(T).Name} at fragment[{key.AsString()}], because that fragment already exists.");
                return MutationResult.FragmentAlreadyExistsFailure;
            }
            InsertResult<T> result = Insert(state, key, requester);
            if (result == null) {
                log.Warning($"{GetType()}.Insert returned null, while this is not allowed.");
                return MutationResult.ResultNullFailure;
            }
            return result;
        }
        protected abstract InsertResult<T> Insert(IReadOnlyDictionary<FragmentKey, Fragment> state, FragmentKey fragmentId, Guid requester);
    }
}
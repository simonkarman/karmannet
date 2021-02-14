using System;
using System.Collections.Generic;

namespace KarmanNet.Karmax {
    public abstract class InsertMutation<T> : Mutation where T : Fragment {
        protected InsertMutation(byte[] bytes) : base(bytes) { }

        internal override MutationResult Mutate(IReadOnlyDictionary<string, Fragment> state, string fragmentId, Guid requester) {
            if (state.ContainsKey(fragmentId)) {
                log.Warning($"Can not {GetType().Name} a {typeof(T).Name} at fragment[{fragmentId}], because that fragment already exists.");
                return MutationResult.FragmentAlreadyExistsFailure;
            }
            InsertResult<T> result = Insert(state, fragmentId, requester);
            if (result == null) {
                log.Warning($"{GetType()}.Insert returned null, while this is not allowed.");
                return MutationResult.ResultNullFailure;
            }
            return result;
        }
        protected abstract InsertResult<T> Insert(IReadOnlyDictionary<string, Fragment> state, string fragmentId, Guid requester);
    }
}
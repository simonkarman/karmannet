using KarmanNet.Networking;
using System;
using System.Collections.Generic;

namespace KarmanNet.Karmax {
    public abstract class Mutation : ByteConstructable {
        protected readonly Logging.Logger log = Logging.Logger.For<Mutation>();

        protected Mutation(byte[] bytes) : base(bytes) { }
        public abstract MutationResult Mutate(IReadOnlyDictionary<string, Fragment> state, string fragmentId, Guid requester);

        public abstract class Insert<T> : Mutation where T : Fragment {
            protected Insert(byte[] bytes) : base(bytes) { }

            public override MutationResult Mutate(IReadOnlyDictionary<string, Fragment> state, string fragmentId, Guid requester) {
                if (state.ContainsKey(fragmentId)) {
                    log.Warning($"{GetType().Name} (insert mutation) cannot insert {typeof(T).Name} ({fragmentId}), because a fragment with that name already exists.");
                    return MutationResult.Failure(MutationFailureReason.FragmentAlreadyExists);
                }
                var instance = Instantiate(requester);
                if (instance == null) {
                    log.Warning($"{GetType().Name} (insert mutation) cannot insert a {typeof(T).Name} with a null value, make sure the Instantiate method of an insert mutation always returns an non-null value.");
                    return MutationResult.Failure(MutationFailureReason.FragmentNull);
                }
                return MutationResult.Success(instance);
            }
            protected abstract T Instantiate(Guid requester);
        }

        public abstract class Update<T> : Mutation where T : Fragment {
            protected Update(byte[] bytes) : base(bytes) { }

            public override sealed MutationResult Mutate(IReadOnlyDictionary<string, Fragment> state, string fragmentId, Guid requester) {
                if (!state.TryGetValue(fragmentId, out Fragment fragment)) {
                    log.Warning($"{GetType().Name} (update mutation) cannot update {typeof(T).Name} with id '{fragmentId}', because it does not exist.");
                    return MutationResult.Failure(MutationFailureReason.FragmentNotFound);
                }
                if (!(fragment is T fragmentT)) {
                    log.Warning($"{GetType().Name} (update mutation) cannot update {fragment.GetType().Name} with id '{fragmentId}', because of a fragment type mismatch, expected a {typeof(T).Name}.");
                    return MutationResult.Failure(MutationFailureReason.FragmentTypeMismatch);
                }
                var instance = Mutate(fragmentT, requester);
                if (instance == null) {
                    log.Warning($"{GetType().Name} (update mutation) cannot update a {typeof(T).Name} to a null value, make sure the Mutate method of an update mutation always returns an non-null value.");
                    return MutationResult.Failure(MutationFailureReason.FragmentNull);
                }
                return MutationResult.Success(instance);
            }
            protected abstract T Mutate(T fragment, Guid requester);
        }

        public abstract class Upsert<T> : Mutation where T : Fragment {
            protected Upsert(byte[] bytes) : base(bytes) { }

            public override sealed MutationResult Mutate(IReadOnlyDictionary<string, Fragment> state, string fragmentId, Guid requester) {
                T instance;
                if (state.TryGetValue(fragmentId, out Fragment fragment)) {
                    if (!(fragment is T fragmentT)) {
                        log.Warning($"{GetType().Name} (upsert mutation) cannot update {fragment.GetType().Name} '{fragmentId}', because of a fragment type mismatch, expected a {typeof(T).Name}.");
                        return MutationResult.Failure(MutationFailureReason.FragmentTypeMismatch);
                    }
                    instance = Mutate(fragmentT, requester);
                } else {
                    instance = Instantiate(requester);
                }
                if (instance == null) {
                    log.Warning($"{GetType().Name} (upsert mutation) cannot insert a {typeof(T).Name} with a null value, make sure the Instantiate and Mutate methods of an upsert mutation always return an non-null value.");
                    return MutationResult.Failure(MutationFailureReason.FragmentNull);
                }
                return MutationResult.Success(instance);
            }
            protected abstract T Instantiate(Guid requester);
            protected abstract T Mutate(T fragment, Guid requester);
        }

        public sealed class Delete : Mutation {
            public Delete(byte[] bytes) : base(bytes) { }
            public Delete(): base(Bytes.Empty) { }

            public override bool IsValid() {
                return true;
            }

            public override MutationResult Mutate(IReadOnlyDictionary<string, Fragment> state, string fragmentId, Guid requester) {
                if (!state.ContainsKey(fragmentId)) {
                    log.Warning($"{GetType().Name} (delete mutation) cannot delete fragment '{fragmentId}', because it does not exist.");
                    return MutationResult.Failure(MutationFailureReason.FragmentNotFound);
                }
                return MutationResult.Delete();
            }
        }
    }
}
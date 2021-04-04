using KarmanNet.Networking;
using KarmanNet.Protocol;
using System;
using System.Collections.Generic;

namespace KarmanNet.Karmax {
    using State = IReadOnlyDictionary<FragmentKey, Fragment>;

    public abstract class Container {
        private readonly static Logging.Logger log = Logging.Logger.For<Container>();
        protected readonly Factory<Mutation> mutationFactory;
        protected readonly Factory<Fragment> fragmentFactory;
        protected readonly Factory<FragmentKey> fragmentKeyFactory;

        private static readonly HashSet<Guid> containers = new HashSet<Guid>();

        public readonly Guid containerId;
        protected bool isAttached = true;
        protected State state;
        public Action<Fragment, State, FragmentKey, Mutation> OnFragmentInsertedCallback;
        public Action<Fragment, State, FragmentKey, Mutation> OnFragmentUpdatedCallback;
        public Action<State, FragmentKey, Mutation> OnFragmentDeletedCallback;
        public Action<State, FragmentKey, Mutation> OnStateChangedCallback;
        public Action<Guid, string> OnMutationFailedCallback;

        protected Container(Guid containerId) {
            // Verify container uniqueness
            if (containers.Contains(containerId)) {
                throw log.ExitError(new Exception($"Trying to attach a Karmax container[{containerId}], while a container with that id is already attached. Only one container is allowed per server/client."));
            }
            containers.Add(containerId);
            this.containerId = containerId;

            // Build factories
            mutationFactory = Factory<Mutation>.BuildFromAllAssemblies();
            fragmentFactory = Factory<Fragment>.BuildFromAllAssemblies();
            fragmentKeyFactory = Factory<FragmentKey>.BuildFromAllAssemblies();

            // Initialize state
            state = new Dictionary<FragmentKey, Fragment>();
        }

        protected void ReleaseContainer() {
            log.Info("Releasing Karmax container");
            containers.Remove(containerId);
            isAttached = false;
        }

        public Guid Request(FragmentKey key, Mutation mutation) {
            if (!isAttached) {
                throw log.ExitError(new Exception("Cannot Request the mutation of a fragment if the Karmax container is no longer attached to the Server."));
            }
            Guid mutationId = Guid.NewGuid();
            byte[] keyBytes = fragmentKeyFactory.GetBytes(key);
            byte[] payload = mutationFactory.GetBytes(mutation);
            MutationPacket mutationPacket = new MutationPacket(mutationId, containerId, keyBytes, payload);
            Request(mutationPacket, mutation);
            return mutationId;
        }
        protected abstract void Request(MutationPacket mutationPacket, Mutation mutation);

        protected bool TryApply(MutationPacket mutationPacket, out Mutation mutation, out MutationResult result) {
            FragmentKey key = fragmentKeyFactory.FromBytes(mutationPacket.GetKey());
            mutation = mutationFactory.FromBytes(mutationPacket.GetPayload());
            result = mutation.Mutate(state, key, mutationPacket.GetRequester());
            if (result.IsFailure()) {
                return false;
            }
            if (result.IsSuccess()) {
                bool isUpdate = state.ContainsKey(key);
                state = state.CloneWith(key, result.GetFragment());
                if (isUpdate) {
                    SafeInvoker.Invoke(log, OnFragmentUpdatedCallback, result.GetFragment(), state, key, mutation);
                } else {
                    SafeInvoker.Invoke(log, OnFragmentInsertedCallback, result.GetFragment(), state, key, mutation);
                }
            } else if (result.IsDelete()) {
                if (state.ContainsKey(key)) {
                    state = state.CloneWithout(key);
                    SafeInvoker.Invoke(log, OnFragmentDeletedCallback, state, key, mutation);
                }
            }
            SafeInvoker.Invoke(log, OnStateChangedCallback, state, key, mutation);
            return true;
        }
    }
}
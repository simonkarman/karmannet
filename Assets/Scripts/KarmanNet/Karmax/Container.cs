using KarmanNet.Networking;
using KarmanNet.Protocol;
using System;
using System.Collections.Generic;

namespace KarmanNet.Karmax {
    public abstract class Container {
        private readonly static Logging.Logger log = Logging.Logger.For<Container>();
        protected readonly Factory<Mutation> mutationFactory;
        protected readonly Factory<Fragment> fragmentFactory;

        private static readonly HashSet<Guid> containers = new HashSet<Guid>();

        public readonly Guid containerId;
        protected bool isAttached = true;
        protected IReadOnlyDictionary<string, Fragment> state;
        public Action<Fragment, IReadOnlyDictionary<string, Fragment>, string, Mutation> OnFragmentInsertedCallback;
        public Action<Fragment, IReadOnlyDictionary<string, Fragment>, string, Mutation> OnFragmentUpdatedCallback;
        public Action<IReadOnlyDictionary<string, Fragment>, string, Mutation> OnFragmentDeletedCallback;
        public Action<IReadOnlyDictionary<string, Fragment>, string, Mutation> OnStateChangedCallback;
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

            // Initialize state
            state = new Dictionary<string, Fragment>();
        }

        protected void ReleaseContainer() {
            log.Info("Releasing Karmax container");
            containers.Remove(containerId);
            isAttached = false;
        }

        public Guid Request(string fragmentId, Mutation mutation) {
            if (!isAttached) {
                throw log.ExitError(new Exception("Cannot Request the mutation of a fragment if the Karmax container is no longer attached to the Server."));
            }
            Guid mutationId = Guid.NewGuid();
            byte[] payload = mutationFactory.GetBytes(mutation);
            MutationPacket mutationPacket = new MutationPacket(mutationId, containerId, fragmentId, payload);
            Request(mutationPacket, mutation);
            return mutationId;
        }
        protected abstract void Request(MutationPacket mutationPacket, Mutation mutation);

        protected bool TryApply(MutationPacket mutationPacket, out Mutation mutation, out MutationResult result) {
            mutation = mutationFactory.FromBytes(mutationPacket.GetPayload());
            result = mutation.Mutate(state, mutationPacket.GetFragmentId(), mutationPacket.GetRequester());
            if (result.IsFailure()) {
                return false;
            }
            if (result.IsSuccess()) {
                bool isUpdate = state.ContainsKey(mutationPacket.GetFragmentId());
                state = state.CloneWith(mutationPacket.GetFragmentId(), result.GetFragment());
                if (isUpdate) {
                    SafeInvoker.Invoke(log, OnFragmentUpdatedCallback, result.GetFragment(), state, mutationPacket.GetFragmentId(), mutation);
                } else {
                    SafeInvoker.Invoke(log, OnFragmentInsertedCallback, result.GetFragment(), state, mutationPacket.GetFragmentId(), mutation);
                }
            } else if (result.IsDelete()) {
                if (state.ContainsKey(mutationPacket.GetFragmentId())) {
                    state = state.CloneWithout(mutationPacket.GetFragmentId());
                    SafeInvoker.Invoke(log, OnFragmentDeletedCallback, state, mutationPacket.GetFragmentId(), mutation);
                }
            }
            SafeInvoker.Invoke(log, OnStateChangedCallback, state, mutationPacket.GetFragmentId(), mutation);
            return true;
        }
    }
}
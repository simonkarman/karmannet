using KarmanNet.Networking;
using System;
using System.Collections.Generic;

namespace KarmanNet.Karmax {
    public abstract class Container {
        private readonly static Logging.Logger log = Logging.Logger.For<Container>();
        protected readonly Factory<Mutation> mutationFactory;
        protected readonly Factory<Fragment> fragmentFactory;

        private static readonly HashSet<Guid> containers = new HashSet<Guid>();

        public readonly Guid id;
        protected IReadOnlyDictionary<string, Fragment> state;
        public Action<IReadOnlyDictionary<string, Fragment>, string, Mutation> OnMutatedCallback;
        public Action<Guid, string> OnMutationFailedCallback;

        public Container(Guid containerId) {
            // Verify container uniqueness
            if (containers.Contains(containerId)) {
                throw log.ExitError(new Exception($"Trying to create a container with id {containerId}, while a container with that id is already in use. Only one container is allowed per server/client."));
            }
            containers.Add(containerId);
            id = containerId;

            // Build factories
            mutationFactory = Factory<Mutation>.BuildFromAllAssemblies();
            fragmentFactory = Factory<Fragment>.BuildFromAllAssemblies();

            // Initialize state
            state = new Dictionary<string, Fragment>();
        }

        protected void ReleaseContainer() {
            log.Info("Releasing Karmax container");
            containers.Remove(id);
        }

        public abstract Guid Request(string fragmentId, Mutation mutation);
    }
}
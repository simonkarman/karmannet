using Networking;
using System;
using System.Collections.Generic;

namespace KarmanProtocol.Karmax {
    public abstract class Container {
        private readonly static Logging.Logger log = Logging.Logger.For<Container>();
        protected readonly Factory<Mutation> mutationFactory;
        protected readonly Factory<Fragment> fragmentFactory;

        private static readonly HashSet<Guid> containers = new HashSet<Guid>();

        public readonly Guid id;
        protected IReadOnlyDictionary<string, Fragment> state;
        public Action<IReadOnlyDictionary<string, Fragment>, string, Mutation> OnMutatedCallback;

        public Container(Guid containerId) {
            // Verify container uniqueness
            if (containers.Contains(containerId)) {
                throw new Exception("Only one Oracle/Replicator instance is allowed per Server/Client.");
            }
            containers.Add(containerId);
            id = containerId;

            // Build factories
            mutationFactory = Factory<Mutation>.BuildFromAllAssemblies();
            fragmentFactory = Factory<Fragment>.BuildFromAllAssemblies();
            foreach (var fragmentType in fragmentFactory.GetTypes()) {
                var identityMethod = fragmentType.GetMethod("Identity");
                if (identityMethod == null || !identityMethod.IsStatic || identityMethod.IsAbstract || identityMethod.ReturnType != fragmentType || identityMethod.GetParameters().Length > 0) {
                    throw log.ExitError(new Exception($"Missing Identity() method in {fragmentType.Name}. It should have the following signature: public static ${fragmentType.Name} Identity();"));
                }
            }

            // Initialize state
            state = new Dictionary<string, Fragment>();
        }

        public abstract Guid Request(string fragmentId, Mutation mutation);
    }
}
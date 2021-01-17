using Networking;
using System;
using System.Collections.Generic;

namespace KarmanProtocol.Karmax {
    public class Replicator {
        private readonly static Logging.Logger log = Logging.Logger.For<Replicator>();
        private readonly Factory<Mutation> mutationFactory;
        private readonly Factory<Fragment> fragmentFactory;
        private readonly KarmanClient client;

        private IReadOnlyDictionary<string, Fragment> state;
        public Action<IReadOnlyDictionary<string, Fragment>, string, Mutation> OnMutatedCallback;

        public Replicator(KarmanClient client) {
            mutationFactory = Factory<Mutation>.BuildFromAllAssemblies();
            fragmentFactory = Factory<Fragment>.BuildFromAllAssemblies();

            this.client = client;
            this.client.OnConnectedCallback += OnConnected;
            this.client.OnPacketReceivedCallback += OnPacketReceived;
        }

        private void OnConnected() {
            state = new Dictionary<string, Fragment>();
        }

        private void OnPacketReceived(Packet packet) {
            if (packet is FragmentPacket fragmentPacket) {
                state = state.ReplacementWith(fragmentPacket.GetId(), fragmentFactory.FromBytes(fragmentPacket.GetPayload()));
            } else if (packet is MutationPacket mutationPacket) {
                Mutation mutation = mutationFactory.FromBytes(mutationPacket.GetPayload());
                MutationResult result = mutation.Mutate(state, mutationPacket.GetFragmentId(), mutationPacket.GetRequester());
                if (result.IsFailure()) {
                    client.Leave("replication failure");
                    return;
                }
                state = state.ReplacementWith(mutationPacket.GetFragmentId(), result.GetFragment());
                SafeInvoker.Invoke(log, "OnMutated", OnMutatedCallback, state, mutationPacket.GetFragmentId(), mutation);
            }
        }

        public void Request(string fragmentId, Mutation mutation) {
            Guid id = Guid.NewGuid();
            byte[] payload = mutationFactory.GetBytes(mutation);
            MutationPacket mutationPacket = new MutationPacket(id, client.id, fragmentId, payload);
            client.Send(mutationPacket);
        }
    }
}
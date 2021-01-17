using Networking;
using System;
using System.Collections.Generic;

namespace KarmanProtocol.Karmax {
    public class Oracle {
        private readonly static Logging.Logger log = Logging.Logger.For<Oracle>();
        public static readonly string ImposterFailureReason = "karmax::imposter";
        private readonly Factory<Mutation> mutationFactory;
        private readonly Factory<Fragment> fragmentFactory;
        private readonly KarmanServer server;

        private IReadOnlyDictionary<string, Fragment> state;
        public Action<IReadOnlyDictionary<string, Fragment>, string, Mutation> OnMutatedCallback;

        public Oracle(KarmanServer server) {
            // TODO: only allow a singular oracle/replicator per server/client (or add an id/name to the oracle)
            mutationFactory = Factory<Mutation>.BuildFromAllAssemblies();
            fragmentFactory = Factory<Fragment>.BuildFromAllAssemblies();

            this.server = server;
            server.OnClientConnectedCallback += OnClientConnected;
            server.OnClientPacketReceivedCallback += OnClientPacketReceived;
            state = new Dictionary<string, Fragment>();
        }

        private void OnClientConnected(Guid clientId) {
            foreach (var kvp in state) {
                server.Send(clientId, new FragmentPacket(kvp.Key, fragmentFactory.GetBytes(kvp.Value)));
            }
        }

        private void OnClientPacketReceived(Guid clientId, Packet packet) {
            if (!(packet is MutationPacket mutationPacket)) {
                return;
            }
            if (clientId != mutationPacket.GetRequester()) {
                server.Send(clientId, new MutationFailedPacket(mutationPacket.GetId(), ImposterFailureReason));
                return;
            }
            HandleMutationPacketFrom(mutationPacket, clientId);
        }

        public void Request(string fragmentId, Mutation mutation) {
            Guid id = Guid.NewGuid();
            byte[] payload = mutationFactory.GetBytes(mutation);
            MutationPacket mutationPacket = new MutationPacket(id, Guid.Empty, fragmentId, payload);
            HandleMutationPacketFrom(mutationPacket, Guid.Empty);
        }

        private void HandleMutationPacketFrom(MutationPacket mutationPacket, Guid requester) {
            Mutation mutation = mutationFactory.FromBytes(mutationPacket.GetPayload());
            MutationResult result = mutation.Mutate(state, mutationPacket.GetFragmentId(), requester);
            if (result.IsFailure()) {
                server.Send(requester, new MutationFailedPacket(mutationPacket.GetId(), result.GetFailureReason()));
                return;
            }
            state = state.ReplacementWith(mutationPacket.GetFragmentId(), result.GetFragment());
            SafeInvoker.Invoke(log, "OnMutated", OnMutatedCallback, state, mutationPacket.GetFragmentId(), mutation);
            server.Broadcast(mutationPacket);
        }
    }
}
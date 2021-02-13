using Networking;
using System;

namespace KarmanProtocol.Karmax {
    public class Oracle : Container {
        private readonly static Logging.Logger log = Logging.Logger.For<Oracle>();
        public static readonly string ImposterFailureReason = "karmax::imposter";
        private readonly KarmanServer server;

        public Oracle(KarmanServer server): base(Guid.Empty) {
            this.server = server;
            server.OnClientConnectedCallback += OnClientConnected;
            server.OnClientPacketReceivedCallback += OnClientPacketReceived;
        }

        private void OnClientConnected(Guid clientId) {
            foreach (var kvp in state) {
                server.Send(clientId, new FragmentPacket(kvp.Key, fragmentFactory.GetBytes(kvp.Value)));
            }
        }

        private void OnClientPacketReceived(Guid clientId, Packet packet) {
            if (packet is MutationPacket mutationPacket) {
                if (clientId != mutationPacket.GetRequester()) {
                    server.Send(clientId, new MutationFailedPacket(mutationPacket.GetId(), ImposterFailureReason));
                    return;
                }
                HandleMutationPacketFrom(mutationPacket, clientId);
            }
        }

        public override Guid Request(string fragmentId, Mutation mutation) {
            Guid id = Guid.NewGuid();
            byte[] payload = mutationFactory.GetBytes(mutation);
            MutationPacket mutationPacket = new MutationPacket(id, Guid.Empty, fragmentId, payload);
            HandleMutationPacketFrom(mutationPacket, Guid.Empty);
            return id;
        }

        private void HandleMutationPacketFrom(MutationPacket mutationPacket, Guid requester) {
            Mutation mutation = mutationFactory.FromBytes(mutationPacket.GetPayload());
            MutationResult result = mutation.Mutate(state, mutationPacket.GetFragmentId(), requester);
            if (result.IsFailure()) {
                server.Send(requester, new MutationFailedPacket(mutationPacket.GetId(), result.GetFailureReason()));
                return;
            }
            state = state.CloneWith(mutationPacket.GetFragmentId(), result.GetFragment());
            SafeInvoker.Invoke(log, "OnMutated", OnMutatedCallback, state, mutationPacket.GetFragmentId(), mutation);
            server.Broadcast(mutationPacket);
        }
    }
}
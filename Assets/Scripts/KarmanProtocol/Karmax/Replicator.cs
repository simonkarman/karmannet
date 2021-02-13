using Networking;
using System;

namespace KarmanProtocol.Karmax {
    public class Replicator : Container {
        private readonly static Logging.Logger log = Logging.Logger.For<Replicator>();
        private readonly KarmanClient client;

        public Replicator(KarmanClient client): base(client.id) {
            this.client = client;
            this.client.OnPacketReceivedCallback += OnPacketReceived;
        }

        private void OnPacketReceived(Packet packet) {
            if (packet is FragmentPacket fragmentPacket) {
                state = state.CloneWith(fragmentPacket.GetId(), fragmentFactory.FromBytes(fragmentPacket.GetPayload()));
            } else if (packet is MutationPacket mutationPacket) {
                Mutation mutation = mutationFactory.FromBytes(mutationPacket.GetPayload());
                MutationResult result = mutation.Mutate(state, mutationPacket.GetFragmentId(), mutationPacket.GetRequester());
                if (result.IsFailure()) {
                    client.Leave("replication failure");
                    return;
                }
                state = state.CloneWith(mutationPacket.GetFragmentId(), result.GetFragment());
                SafeInvoker.Invoke(log, "OnMutated", OnMutatedCallback, state, mutationPacket.GetFragmentId(), mutation);
            }
        }

        public override Guid Request(string fragmentId, Mutation mutation) {
            Guid id = Guid.NewGuid();
            byte[] payload = mutationFactory.GetBytes(mutation);
            MutationPacket mutationPacket = new MutationPacket(id, client.id, fragmentId, payload);
            client.Send(mutationPacket);
            return id;
        }
    }
}
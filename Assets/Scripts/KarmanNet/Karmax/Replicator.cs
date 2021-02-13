using KarmanNet.Networking;
using KarmanNet.Protocol;
using System;
using System.Collections.Generic;

namespace KarmanNet.Karmax {
    public class Replicator : Container {
        private readonly static Logging.Logger log = Logging.Logger.For<Replicator>();
        private readonly KarmanClient client;

        private readonly Dictionary<Guid, string> pending = new Dictionary<Guid, string>(); 

        public Replicator(KarmanClient client): base(client.id) {
            this.client = client;
            this.client.OnPacketReceivedCallback += OnPacketReceived;
            this.client.OnLeftCallback += OnLeft;
        }

        private void OnLeft(string reason) {
            ReleaseContainer();
        }

        private void OnPacketReceived(Packet packet) {
            if (packet is FragmentPacket fragmentPacket) {
                state = state.CloneWith(fragmentPacket.GetId(), fragmentFactory.FromBytes(fragmentPacket.GetPayload()));
            } else if (packet is MutationPacket mutationPacket) {
                // If mutation was requested by me, then this request is no longer pending
                if (mutationPacket.GetRequester().Equals(client.id)) {
                    pending.Remove(mutationPacket.GetId());
                }

                // Perform the mutation
                Mutation mutation = mutationFactory.FromBytes(mutationPacket.GetPayload());
                MutationResult result = mutation.Mutate(state, mutationPacket.GetFragmentId(), mutationPacket.GetRequester());
                if (result.IsFailure()) {
                    // This should never happen since the mutation has already been succesfully applied at the oracle. If it does happen
                    //  the state is probably corrupt and that is a good reason to directly leave the server.
                    client.Leave("Fatal Karmax replication failure - state corrupted");
                    return;
                }

                // Apply the mutation to the local state
                if (result.IsSuccess()) {
                    state = state.CloneWith(mutationPacket.GetFragmentId(), result.GetFragment());
                } else if (result.IsDelete()) {
                    state = state.CloneWithout(mutationPacket.GetFragmentId());
                }
                SafeInvoker.Invoke(log, "OnMutated", OnMutatedCallback, state, mutationPacket.GetFragmentId(), mutation);
            } else if (packet is MutationFailedPacket mutationFailedPacket) {
                var id = mutationFailedPacket.GetId();
                log.Warning("Mutation with id {0} failed.\nDetails: {1}", id, pending[id]);
                pending.Remove(id);
                SafeInvoker.Invoke(log, "OnMutationFailed", OnMutationFailedCallback, id, mutationFailedPacket.GetFailureReason());
            }
        }

        public override Guid Request(string fragmentId, Mutation mutation) {
            Guid id = Guid.NewGuid();
            byte[] payload = mutationFactory.GetBytes(mutation);
            MutationPacket mutationPacket = new MutationPacket(id, client.id, fragmentId, payload);
            client.Send(mutationPacket);
            pending.Add(id, $"{mutation.GetType().Name} was requested for fragment {fragmentId}.\n{Environment.StackTrace}");
            return id;
        }
    }
}
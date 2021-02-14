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
            client.OnPacketReceivedCallback -= OnPacketReceived;
            client.OnLeftCallback -= OnLeft;
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
                if (!TryApply(mutationPacket, out Mutation mutation, out MutationResult result)) {
                    // This should never happen since the mutation has already been succesfully applied at the Oracle. This
                    //  indicates that replication state is corrupt. This is a good reason to directly disconnect from the server.
                    string message = "Fatal Karmax replication failure";
                    log.Error($"{message}. Corrupt state detected while applying {mutation.GetType().Name} on fragment[{mutationPacket.GetFragmentId()}]. Reason: {result.GetFailureReason()}");
                    client.Leave(message);
                    return;
                }
            } else if (packet is MutationFailedPacket mutationFailedPacket) {
                var mutationId = mutationFailedPacket.GetId();
                log.Warning($"Mutation[{mutationId}] failed. Reason: {mutationFailedPacket.GetFailureReason()}. Details: {pending[mutationId]}");
                pending.Remove(mutationId);
                SafeInvoker.Invoke(log, OnMutationFailedCallback, mutationId, mutationFailedPacket.GetFailureReason());
            }
        }

        protected override void Request(MutationPacket mutationPacket, Mutation mutation) {
            client.Send(mutationPacket);
            pending.Add(mutationPacket.GetId(), $"{mutation.GetType().Name} on fragment[{mutationPacket.GetFragmentId()}].\nStackTrace: {Environment.StackTrace}");
        }
    }
}
using KarmanNet.Networking;
using KarmanNet.Protocol;
using System;

namespace KarmanNet.Karmax {
    public class Oracle : Container {
        private readonly static Logging.Logger log = Logging.Logger.For<Oracle>();
        private readonly KarmanServer server;

        public Oracle(KarmanServer server): base(Guid.Empty) {
            this.server = server;
            server.OnClientConnectedCallback += OnClientConnected;
            server.OnClientPacketReceivedCallback += OnClientPacketReceived;
            server.OnShutdownCallback += OnShutdown;
        }

        private void OnShutdown() {
            server.OnClientConnectedCallback -= OnClientConnected;
            server.OnClientPacketReceivedCallback -= OnClientPacketReceived;
            server.OnShutdownCallback -= OnShutdown;
            ReleaseContainer();
        }

        private void OnClientConnected(Guid clientId) {
            foreach (var kvp in state) {
                server.Send(clientId, new FragmentPacket(fragmentKeyFactory.GetBytes(kvp.Key), fragmentFactory.GetBytes(kvp.Value)));
            }
        }

        private void OnClientPacketReceived(Guid clientId, Packet packet) {
            if (packet is MutationPacket mutationPacket) {
                if (clientId != mutationPacket.GetRequester()) {
                    server.Send(clientId, new MutationFailedPacket(mutationPacket.GetId(), MutationResult.ImposterFailure.GetFailureReason()));
                    return;
                }
                HandleMutationBy(mutationPacket, clientId);
            }
        }

        protected override void Request(MutationPacket mutationPacket, Mutation mutation) {
            HandleMutationBy(mutationPacket, Guid.Empty);
        }

        private void HandleMutationBy(MutationPacket mutationPacket, Guid requester) {
            if (!TryApply(mutationPacket, out Mutation mutation, out MutationResult result)) {
                if (requester.Equals(Guid.Empty)) {
                    log.Warning($"Mutation[{mutationPacket.GetId()}] failed. Reason: {result.GetFailureReason()}. Details: {mutation.GetType().Name} on fragment[{fragmentKeyFactory.FromBytes(mutationPacket.GetKey()).AsString()}].\nStackTrace: {Environment.StackTrace}");
                    SafeInvoker.Invoke(log, OnMutationFailedCallback, mutationPacket.GetId(), result.GetFailureReason());
                } else {
                    server.Send(requester, new MutationFailedPacket(mutationPacket.GetId(), result.GetFailureReason()));
                }
                return;
            }
            server.Broadcast(mutationPacket);
        }
    }
}
using Networking;
using System.Collections.Generic;

namespace KarmanProtocol.ORPattern {
    public class Replicator<T, ImmutableT>: SharedStateContainer<ImmutableT> where T : IReplicatorSharedState<ImmutableT> {
        private readonly string sharedStateIdentifier;
        private readonly KarmanClient client;
        private readonly T state;

        public Replicator(string sharedStateIdentifier, KarmanClient client, T initialState) {
            this.sharedStateIdentifier = sharedStateIdentifier;
            state = initialState;

            this.client = client;
            client.OnPacketReceivedCallback += OnPacketReceived;
        }

        private void OnPacketReceived(Packet packet) {
            if (!(packet is IStateChangePacket stateChangePacket)) {
                log.Trace("Ignored a {0} packet since the packet is not a IStateChangePacket", packet.GetType().Name);
                return;
            }
            if (!sharedStateIdentifier.Equals(stateChangePacket.GetSharedStateIdentifier())) {
                log.Trace("Ignored a {0} packet since the shared state identifier is {1}", packet.GetType().Name, stateChangePacket.GetSharedStateIdentifier());
                return;
            }
            if (packet is StateChangeFailedEvent stateChangeFailedEvent) {
                StateChangeFailed(GetState(), stateChangeFailedEvent.GetPacketName(), stateChangeFailedEvent.GetReason());
                return;
            } else if (packet is StateChangeEvent stateChangeEvent) {
                log.Trace("Received a {0} packet", packet.GetType().Name);
                ImmutableT oldState = GetState();
                state.Apply(stateChangeEvent);
                StateChanged(GetState(), oldState, stateChangeEvent);
            } else {
                log.Error("Failed to handle {0} packet", packet.GetType().Name);
            }
        }

        public override ImmutableT GetState() {
            if (EqualityComparer<T>.Default.Equals(state, default)) {
                return default;
            }
            return state.ToImmutableClone();
        }

        public override bool RequestStateChange(StateChangeRequest stateChangeRequest) {
            if (!sharedStateIdentifier.Equals(stateChangeRequest.GetSharedStateIdentifier())) {
                return false;
            }
            client.Send(stateChangeRequest);
            return true;
        }
    }
}

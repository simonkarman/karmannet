using Networking;
using System.Collections.Generic;

namespace KarmanProtocol.ORPattern {
    public sealed class Replicator<MutableT, ImmutableT>: SharedStateContainer<ImmutableT> where MutableT : ISharedState<ImmutableT> {
        private readonly string sharedStateIdentifier;
        private readonly KarmanClient client;
        private MutableT state;

        public Replicator(KarmanClient client, MutableT initialState) {
            sharedStateIdentifier = typeof(ImmutableT).FullName;
            state = initialState;

            this.client = client;
            client.OnPacketReceivedCallback += OnPacketReceived;
        }

        private void OnPacketReceived(Packet packet) {
            if (packet is StateChangeFailedEvent stateChangeFailedEvent && stateChangeFailedEvent.GetSharedStateIdentifier().Equals(sharedStateIdentifier)) {
                StateChangeFailed(GetState(), stateChangeFailedEvent.GetPacketName(), stateChangeFailedEvent.GetReason());
                return;
            }

            if (!(packet is SharedStatePacket<ImmutableT>)) {
                log.Trace("Ignored a {0} packet since the packet is not a SharedStatePacket<{1}>", packet.GetType().Name, typeof(ImmutableT).Name);
                return;
            }
            log.Info("Received a {0} packet", packet.GetType().Name);
            
            ImmutableT oldState = GetState();
            if (packet is StateInitializationPacket<MutableT, ImmutableT> entireStateChangedEvent) {
                state = entireStateChangedEvent.ToState();
                StateInitialized(GetState());

            } else if (packet is StateChangedEvent<ImmutableT> stateChangedEvent) {
                state.Apply(stateChangedEvent);
                StateChanged(GetState(), oldState, stateChangedEvent);

            } else {
                log.Error("Failed to handle {0} packet", packet.GetType().Name);
            }
        }

        public override ImmutableT GetState() {
            if (EqualityComparer<MutableT>.Default.Equals(state, default)) {
                return default;
            }
            return state.ToValue();
        }

        public override void RequestStateChange(ChangeStateRequest<ImmutableT> stateChangeRequest) {
            log.Info("Sending a {0} packet", stateChangeRequest.GetType().Name);
            client.Send(stateChangeRequest);
        }
    }
}

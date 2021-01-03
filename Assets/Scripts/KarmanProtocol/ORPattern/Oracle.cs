using Networking;
using System;
using System.Collections.Generic;

namespace KarmanProtocol.ORPattern {
    public class Oracle<T, ImmutableT>: SharedStateContainer<ImmutableT> where T : IOracleSharedState<ImmutableT> {
        private readonly string sharedStateIdentifier;
        private readonly KarmanServer server;
        private readonly T state;

        public Oracle(string sharedStateIdentifier, KarmanServer server, T initialState) {
            this.sharedStateIdentifier = sharedStateIdentifier;
            state = initialState;

            this.server = server;
            server.OnClientConnectedCallback += OnClientConnected;
            server.OnClientPacketReceivedCallback += OnClientPacketReceived;
        }

        private void OnClientConnected(Guid clientId) {
            var entirePacket = state.GetEntirePacket();
            log.Info("Sending an entire packet of type {0} to the client that just connected with id {1}", entirePacket.GetType().Name, clientId);
            server.Send(clientId, entirePacket);
        }

        private void OnClientPacketReceived(Guid clientId, Packet packet) {
            if (!(packet is StateChangeRequest stateChangeRequest)) {
                log.Trace("Ignored a {0} packet since the packet is not a StateChangeRequest", packet.GetType().Name);
                return;
            }
            if (!sharedStateIdentifier.Equals(stateChangeRequest.GetSharedStateIdentifier())) {
                log.Trace("Ignored a {0} packet since the shared state identifier is {1}", packet.GetType().Name, stateChangeRequest.GetSharedStateIdentifier());
                return;
            }
            log.Info("Received a {0} packet", packet.GetType().Name);
            StateChangeResult result = RequestStateChange(stateChangeRequest, clientId);
            if (result.IsError) {
                log.Info("Sending a StateChangeFailedEvent packet");
                server.Send(clientId, new StateChangeFailedEvent(sharedStateIdentifier, packet.GetType().FullName, result.GetErrorReason()));
            }
        }

        public override bool RequestStateChange(StateChangeRequest stateChangeRequest) {
            if (!sharedStateIdentifier.Equals(stateChangeRequest.GetSharedStateIdentifier())) {
                return false;
            }
            StateChangeResult result = RequestStateChange(stateChangeRequest, Guid.Empty);
            if (result.IsError) {
                StateChangeFailed(GetState(), stateChangeRequest.GetType().FullName, result.GetErrorReason());
            }
            return true;
        }

        private StateChangeResult RequestStateChange(StateChangeRequest stateChangeRequest, Guid requester) {
            ImmutableT oldState = GetState();
            StateChangeResult result = state.TryHandle(stateChangeRequest, requester);
            if (result.IsOk) {
                log.Info("Sending a {0} packet after successfully handling a state change from {1}", result.GetStateChangeEvent().GetType().Name, requester);
                server.Broadcast(result.GetStateChangeEvent());
                StateChanged(GetState(), oldState, result.GetStateChangeEvent());
            } else {
                log.Warning("Failed to handle a state change request of type {0} from {1}. Reason: {2}", stateChangeRequest.GetType().Name, requester, result.GetErrorReason());
            }
            return result;
        }

        public override ImmutableT GetState() {
            if (EqualityComparer<T>.Default.Equals(state, default)) {
                return default;
            }
            return state.ToImmutableClone();
        }
    }
}
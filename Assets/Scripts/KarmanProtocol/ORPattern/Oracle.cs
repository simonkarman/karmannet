using Networking;
using System;
using System.Collections.Generic;

namespace KarmanProtocol.ORPattern {
    public sealed class Oracle<MutableT, ImmutableT> : SharedStateContainer<ImmutableT> where MutableT : IOracleSharedState<MutableT, ImmutableT> {
        private readonly string sharedStateIdentifier;
        private readonly KarmanServer server;
        private readonly MutableT state;

        public Oracle(KarmanServer server, MutableT initialState) {
            sharedStateIdentifier = typeof(ImmutableT).FullName;
            state = initialState;

            this.server = server;
            server.OnClientConnectedCallback += OnClientConnected;
            server.OnClientPacketReceivedCallback += OnClientPacketReceived;
        }

        private void OnClientConnected(Guid clientId) {
            StateInitializationPacket<MutableT, ImmutableT> entirePacket = state.GetStateInitializationPacket();
            log.Info("Sending an entire packet of type {0} to the client that just connected with id {1}", entirePacket.GetType().Name, clientId);
            server.Send(clientId, entirePacket);
        }

        private void OnClientPacketReceived(Guid clientId, Packet packet) {
            if (!(packet is ChangeStateRequest<ImmutableT> stateChangeRequest)) {
                log.Trace("Ignored a {0} packet since the packet is not a StateChangeRequest<{1}>", packet.GetType().Name, typeof(ImmutableT).Name);
                return;
            }
            log.Info("Received a {0} packet", packet.GetType().Name);
            StateChangeResult<ImmutableT> result = RequestStateChange(stateChangeRequest, clientId);
            if (result.IsError) {
                log.Info("Sending a StateChangeFailedEvent packet");
                server.Send(clientId, new StateChangeFailedEvent(stateChangeRequest.GetRequestId(), sharedStateIdentifier, packet.GetType().FullName, result.GetErrorReason()));
            }
        }

        public override void RequestStateChange(ChangeStateRequest<ImmutableT> stateChangeRequest) {
            StateChangeResult<ImmutableT> result = RequestStateChange(stateChangeRequest, Guid.Empty);
            if (result.IsError) {
                StateChangeFailed(GetState(), stateChangeRequest.GetType().FullName, result.GetErrorReason());
            }
        }

        private StateChangeResult<ImmutableT> RequestStateChange(ChangeStateRequest<ImmutableT> stateChangeRequest, Guid requester) {
            ImmutableT oldState = GetState();
            StateChangeResult<ImmutableT> result = state.Verify(stateChangeRequest, requester);
            if (result.IsOk) {
                state.Apply(result.GetStateChangedEvent());
                log.Info("Sending a {0} packet after successfully handling a state change from {1}", result.GetStateChangedEvent().GetType().Name, requester);
                server.Broadcast(result.GetStateChangedEvent());
                StateChanged(GetState(), oldState, result.GetStateChangedEvent());
            } 
            if (result.IsError) {
                log.Warning("Failed to handle a state change request of type {0} from {1}. Reason: {2}", stateChangeRequest.GetType().Name, requester, result.GetErrorReason());
            }
            return result;
        }

        public override ImmutableT GetState() {
            if (EqualityComparer<MutableT>.Default.Equals(state, default)) {
                return default;
            }
            return state.ToValue();
        }
    }
}
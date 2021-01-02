using System;

namespace KarmanProtocol.ORPattern {
    public abstract class SharedStateContainer<ImmutableT> {
        protected readonly Logging.Logger log;

        public SharedStateContainer() {
            log = new Logging.Logger(GetType());
        }

        protected void StateChanged(ImmutableT newState, ImmutableT oldState, StateChangeEvent stateChangeEvent) {
            SafeInvoker.Invoke(log, "OnStateChanged", OnStateChanged, newState, oldState, stateChangeEvent);
        }

        protected void StateChangeFailed(ImmutableT newState, string packetTypeName, string reason) {
            SafeInvoker.Invoke(log, "OnStateChangeFailed", OnStateChangeFailed, newState, packetTypeName, reason);
        }

        public Action<ImmutableT, ImmutableT, StateChangeEvent> OnStateChanged;
        public Action<ImmutableT, string, string> OnStateChangeFailed;
        public abstract ImmutableT GetState();
        public abstract bool RequestStateChange(StateChangeRequest stateChangeRequest);
    }
}
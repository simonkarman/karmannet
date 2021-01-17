using System;

namespace KarmanProtocol.ORPattern {
    public abstract class SharedStateContainer<ImmutableT> {
        protected readonly Logging.Logger log;

        public SharedStateContainer() {
            log = new Logging.Logger(GetType());
        }

        protected void StateInitialized(ImmutableT state) {
            SafeInvoker.Invoke(log, "OnStateInitialized", OnStateInitialized, state);
        }

        protected void StateChanged(ImmutableT newState, ImmutableT oldState, StateChangedEvent<ImmutableT> changeOrigin) {
            SafeInvoker.Invoke(log, "OnStateChanged", OnStateChanged, newState, oldState, changeOrigin);
        }

        protected void StateChangeFailed(ImmutableT newState, string packetTypeName, string reason) {
            SafeInvoker.Invoke(log, "OnStateChangeFailed", OnStateChangeFailed, newState, packetTypeName, reason);
        }

        public Action<ImmutableT> OnStateInitialized;
        public Action<ImmutableT, ImmutableT, StateChangedEvent<ImmutableT>> OnStateChanged;
        public Action<ImmutableT, string, string> OnStateChangeFailed;
        public abstract ImmutableT GetState();
        public abstract void RequestStateChange(ChangeStateRequest<ImmutableT> stateChangeRequest);
    }
}
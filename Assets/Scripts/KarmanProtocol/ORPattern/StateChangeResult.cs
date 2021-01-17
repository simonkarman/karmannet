using System;

namespace KarmanProtocol.ORPattern {
    public class StateChangeResult<ImmutableT> {
        private readonly string errorReason;
        private readonly StateChangedEvent<ImmutableT> stateChangedEvent;

        private StateChangeResult(string errorReason, StateChangedEvent<ImmutableT> stateChangedEvent) {
            this.errorReason = errorReason;
            this.stateChangedEvent = stateChangedEvent;
        }

        public static StateChangeResult<ImmutableT> Ok(StateChangedEvent<ImmutableT> stateChangeEvent) {
            if (stateChangeEvent == null) {
                throw new ArgumentNullException("stateChangeEvent");
            }
            return new StateChangeResult<ImmutableT>(null, stateChangeEvent);
        }

        public static StateChangeResult<ImmutableT> Error(string errorReason) {
            if (errorReason == null) {
                throw new ArgumentNullException("errorReason");
            }
            return new StateChangeResult<ImmutableT>(errorReason, null);
        }

        public static StateChangeResult<ImmutableT> UnknownRequest() {
            return new StateChangeResult<ImmutableT>("unknown request", null);
        }

        public static StateChangeResult<ImmutableT> Unauthorized() {
            return new StateChangeResult<ImmutableT>("unauthorized", null);
        }

        public bool IsOk {
            get { return stateChangedEvent != null; }
        }

        public bool IsError {
            get { return errorReason != null; }
        }

        public string GetErrorReason() {
            return errorReason;
        }

        public StateChangedEvent<ImmutableT> GetStateChangedEvent() {
            return stateChangedEvent;
        }
    }
}

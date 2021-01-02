using System;

namespace KarmanProtocol.ORPattern {
    public class StateChangeResult {
        private readonly string errorReason;
        private readonly StateChangeEvent stateChangeEvent;

        private StateChangeResult(string errorReason, StateChangeEvent stateChangeEvent) {
            this.errorReason = errorReason;
            this.stateChangeEvent = stateChangeEvent;
        }

        public static StateChangeResult Ok(StateChangeEvent stateChangeEvent) {
            if (stateChangeEvent == null) {
                throw new ArgumentNullException("stateChangeEvent");
            }
            return new StateChangeResult(null, stateChangeEvent);
        }

        public static StateChangeResult Error(string errorReason) {
            if (errorReason == null) {
                throw new ArgumentNullException("errorReason");
            }
            return new StateChangeResult(errorReason, null);
        }

        public static StateChangeResult UnknownRequest() {
            return new StateChangeResult("unknown request", null);
        }

        public static StateChangeResult Unauthorized() {
            return new StateChangeResult("unauthorized", null);
        }

        public bool IsOk {
            get { return errorReason == null; }
        }

        public bool IsError {
            get { return errorReason != null; }
        }

        public string GetErrorReason() {
            return errorReason;
        }

        public StateChangeEvent GetStateChangeEvent() {
            return stateChangeEvent;
        }
    }
}

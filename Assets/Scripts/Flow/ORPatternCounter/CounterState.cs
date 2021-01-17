using KarmanProtocol.ORPattern;
using System;

namespace ORPatternExample {
    public class CounterState : IOracleSharedState<CounterState, Counter> {
        private int value = 0;

        public CounterState(int value) {
            this.value = value;
        }

        public StateInitializationPacket<CounterState, Counter> GetStateInitializationPacket() {
            return new CounterStateInitializationPacket(Guid.Empty, value);
        }

        public string GetStateIdentifier() {
            return typeof(Counter).FullName;
        }

        public Counter ToValue() {
            return new Counter(value);
        }

        public StateChangeResult<Counter> Verify(ChangeStateRequest<Counter> stateChangeRequest, Guid requester) {
            if (stateChangeRequest is Increment increment) {
                return StateChangeResult<Counter>.Ok(new CounterValueChangedEvent(requester, value + increment.GetAmount()));
            }
            if (stateChangeRequest is Multiply multiply) {
                return StateChangeResult<Counter>.Ok(new CounterValueChangedEvent(requester, value * multiply.GetProduct()));
            }
            return StateChangeResult<Counter>.UnknownRequest();
        }

        public void Apply(StateChangedEvent<Counter> stateChangeEvent) {
            if (stateChangeEvent is CounterValueChangedEvent valueChanged) {
                value = valueChanged.GetValue();
            }
        }
    }
}

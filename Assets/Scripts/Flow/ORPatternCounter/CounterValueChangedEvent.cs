using KarmanProtocol.ORPattern;
using System;
using Networking;

namespace ORPatternExample {
    public class CounterValueChangedEvent : StateChangedEvent<Counter> {
        private readonly int value;

        public CounterValueChangedEvent(byte[] bytes) : base(bytes) {
            value = ReadInt();
        }

        public CounterValueChangedEvent(Guid requestId, int value) : base(requestId, Bytes.Of(value)) {
            this.value = value;
        }

        public override bool IsValid() {
            return true;
        }

        public int GetValue() {
            return value;
        }
    }
}
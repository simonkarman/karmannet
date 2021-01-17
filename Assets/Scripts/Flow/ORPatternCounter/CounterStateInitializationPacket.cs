using KarmanProtocol.ORPattern;
using System;
using Networking;

namespace ORPatternExample {
    public class CounterStateInitializationPacket : StateInitializationPacket<CounterState, Counter> {
        private readonly int value;

        public CounterStateInitializationPacket(byte[] bytes) : base(bytes) {
            value = ReadInt();
        }

        public CounterStateInitializationPacket(Guid requestId, int value) : base(requestId, Bytes.Of(value)) {
            this.value = value;
        }

        public override bool IsValid() {
            return true;
        }

        public override CounterState ToState() {
            return new CounterState(value);
        }
    }
}

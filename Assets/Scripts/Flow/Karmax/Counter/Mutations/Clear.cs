using KarmanNet.Karmax;
using KarmanNet.Networking;
using System;
using System.Collections.Generic;

namespace KarmaxCounter {
    public class Clear : DeleteMutation<Counter> {
        public Clear(byte[] bytes) : base(bytes) {}
        public Clear() : base(Bytes.Empty) {}

        public override bool IsValid() {
            return true;
        }

        protected override DeleteResult Delete(Counter counter, IReadOnlyDictionary<string, Fragment> state, string fragmentId, Guid requester) {
            return DeleteResult.Delete();
        }
    }
}

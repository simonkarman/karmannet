using Networking;
using System;
using System.Collections.Generic;

namespace KarmanProtocol.Karmax {
    public abstract class Mutation : ByteConstructable {
        protected Mutation(byte[] bytes) : base(bytes) { }
        public abstract MutationResult Mutate(IReadOnlyDictionary<string, Fragment> state, string fragmentId, Guid requester);
    }
}
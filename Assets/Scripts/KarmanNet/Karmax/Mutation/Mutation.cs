using KarmanNet.Networking;
using System;
using System.Collections.Generic;

namespace KarmanNet.Karmax {
    public abstract class Mutation : ByteConstructable {
        protected readonly Logging.Logger log = Logging.Logger.For<Mutation>();

        protected Mutation(byte[] bytes) : base(bytes) { }
        internal abstract MutationResult Mutate(IReadOnlyDictionary<FragmentKey, Fragment> state, FragmentKey key, Guid requester);
    }
}
using System;

namespace KarmanProtocol.ORPattern {
    public interface IOracleSharedState<ImmutableT> : ISharedState<ImmutableT> {
        StateChangeResult TryHandle(StateChangeRequest stateChangeRequest, Guid requester);
    }
}

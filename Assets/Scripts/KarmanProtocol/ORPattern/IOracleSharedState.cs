using System;

namespace KarmanProtocol.ORPattern {
    public interface IOracleSharedState<MutableT, ImmutableT> : ISharedState<ImmutableT> {
        StateInitializationPacket<MutableT, ImmutableT> GetStateInitializationPacket();
        StateChangeResult<ImmutableT> Verify(ChangeStateRequest<ImmutableT> stateChangeRequest, Guid requester);
    }
}

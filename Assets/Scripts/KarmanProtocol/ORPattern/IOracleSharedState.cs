using System;

namespace KarmanProtocol.ORPattern {
    public interface IOracleSharedState<MutableT, ImmutableT> : ISharedState<ImmutableT> {
        StateInitializationPacket<MutableT, ImmutableT> GetEntirePacket();
        StateChangeResult<ImmutableT> Verify(ChangeStateRequest<ImmutableT> stateChangeRequest, Guid requester);
    }
}

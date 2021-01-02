namespace KarmanProtocol.ORPattern {
    public interface IReplicatorSharedState<ImmutableT> : ISharedState<ImmutableT> {
        void Apply(StateChangeEvent stateChangeEvent);
    }
}

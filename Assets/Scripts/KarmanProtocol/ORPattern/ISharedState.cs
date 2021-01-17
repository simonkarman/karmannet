namespace KarmanProtocol.ORPattern {
    public interface ISharedState<ImmutableT> {
        string GetStateIdentifier();
        ImmutableT ToValue();
        void Apply(StateChangedEvent<ImmutableT> stateChangeEvent);
    }
}

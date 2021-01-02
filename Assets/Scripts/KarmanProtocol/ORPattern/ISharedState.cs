namespace KarmanProtocol.ORPattern {
    public interface ISharedState<ImmutableT> {
        string GetStateIdentifier();
        ImmutableT ToImmutableClone();
        StateChangeEvent GetEntirePacket();
    }
}

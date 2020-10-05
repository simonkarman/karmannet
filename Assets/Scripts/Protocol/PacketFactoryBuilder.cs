using Networking;

public class PacketFactoryBuilder {

    public static PacketFactory GetPacketFactory() {
        PacketFactory packetFactory = new PacketFactory();
        packetFactory.Assign(10, typeof(MessagePacket));
        packetFactory.Assign(11, typeof(RequestUsernamePacket));
        packetFactory.Assign(12, typeof(ProvideUsernamePacket));
        return packetFactory;
    }
}

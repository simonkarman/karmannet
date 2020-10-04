using Networking;

public class PacketFactoryBuilder {

    public static PacketFactory GetPacketFactory() {
        PacketFactory packetFactory = new PacketFactory();
        packetFactory.Assign(10, typeof(MessagePacket));
        return packetFactory;
    }
}

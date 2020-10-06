using System;
using System.Linq;
using Networking;

public class PacketFactoryBuilder {
    public static PacketFactory FromAssemblies() {
        PacketFactory packetFactory = new PacketFactory();
        foreach (var type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes())) {
            if (typeof(Packet).IsAssignableFrom(type) && type.IsClass && !type.IsAbstract) {
                packetFactory.Assign(type);
            }
        }
        return packetFactory;
    }
}

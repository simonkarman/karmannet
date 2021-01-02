using Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

/*
 * Example usage of PacketFactory class
 * ---
 * 
 * PacketFactory packetFactory = new PacketFactory();
 * packetFactory.Assign(10, typeof(MessagePacket));
 *
 * byte[] bytes = packetFactory.GetBytes(new MessagePacket("Hello World!"));
 * Packet packet = packetFactory.FromBytes(bytes);
 * if (packet is MessagePacket messagePacket) {
 *     Console.WriteLine(messagePacket.GetMessage());
 * }
*/
namespace Networking {
    public class PacketFactory {
        private readonly static Logger log = Logger.For<PacketFactory>();

        private readonly Dictionary<Type, int> identifiers = new Dictionary<Type, int>();
        private readonly Dictionary<int, ConstructorInfo> constructors = new Dictionary<int, ConstructorInfo>();

        public static PacketFactory BuildFromAllAssemblies() {
            PacketFactory packetFactory = new PacketFactory();
            foreach (var type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes())) {
                if (typeof(Packet).IsAssignableFrom(type) && type.IsClass && !type.IsAbstract) {
                    packetFactory.Assign(type);
                }
            }
            log.Info("Constructed a new PacketFactory from all assemblies: {0}", packetFactory);
            return packetFactory;
        }

        public void Assign(Type type) {
            Assign(type.FullName.GetHashCode(), type);
        }

        private void Assign(int identifier, Type type) {
            if (!type.IsClass || type.IsAbstract || !typeof(Packet).IsAssignableFrom(type)) {
                throw log.ExitError(new PacketFactoryAssignException(string.Format(
                    "Cannot assign type {0}, because (a) it is not a class, or (b) it is an abstract class, or (c) it does not derive from the Packet class.",
                    type.Name,
                    identifier
                )));
            }
            ConstructorInfo constructor = type.GetConstructor(new[] { typeof(byte[]) });
            if (constructor == null) {
                throw log.ExitError(new PacketFactoryAssignException(string.Format(
                    "Cannot assign type {0}, because no constructor exists in {0} that takes a single parameter of type byte[].",
                    type.Name,
                    identifier
                )));
            }
            if (constructors.TryGetValue(identifier, out ConstructorInfo existingConstructorInfo)) {
                throw log.ExitError(new PacketFactoryAssignException(string.Format(
                    "Cannot assign type {0} to identifier {1}, because the identifier {1} has already been assigned to type {2}.",
                    type.Name,
                    identifier,
                    existingConstructorInfo.DeclaringType.Name
                )));
            }
            if (identifiers.TryGetValue(type, out int existingPacketIdentifier)) {
                throw log.ExitError(new PacketFactoryAssignException(string.Format(
                    "Cannot assign type {0} to identifier {1}, because the type {0} has already been assigned to identifier {2}.",
                    type.Name,
                    identifier,
                    existingPacketIdentifier
                )));
            }
            identifiers.Add(type, identifier);
            constructors.Add(identifier, constructor);
        }

        private void ValidateOrThrow(string action, Packet packet) {
            bool isValid;
            try {
                isValid = packet.IsValid();
            } catch (Exception ex) {
                log.Error("An exception occured while trying to validate {0}: {1}", packet.GetType().Name, ex.Message);
                isValid = false;
            }
            if (!isValid) {
                throw log.ExitError(new PacketInvalidException(action, packet.GetType().Name));
            }
        }

        public byte[] GetBytes(Packet packet) {
            ValidateOrThrow("Tried to send", packet);
            Type type = packet.GetType();
            if (!identifiers.TryGetValue(type, out int identifier)) {
                throw log.ExitError(new PacketFactoryAssignException(string.Format(
                    "Cannot get bytes for the given packet, because identifier of type {0} cannot be found. Ensure that you first assign a packet identifier to the packet type using PacketFactory.Assign().",
                    type.Name
                )));
            }
            try {
                byte[] prefix = BitConverter.GetBytes(identifier);
                byte[] data = packet.GetBytes();

                int length = prefix.Length + data.Length;
                byte[] bytes = new byte[length];
                prefix.CopyTo(bytes, 0);
                data.CopyTo(bytes, prefix.Length);

                return bytes;
            } catch (Exception) {
                throw log.ExitError(new PacketFactoryBytesException(string.Format("An exception occurred trying to get bytes of {0}", packet.GetType().Name)));
            }
        }

        public Packet FromBytes(byte[] bytes) {
            if (bytes == null | bytes.Length < 4) {
                throw log.ExitError(new PacketFactoryBytesException(string.Format("Cannot get packet for the given bytes, because the byte array has a {0}.", bytes == null
                    ? "value of null"
                    : string.Format("length of {0} byte(s), while it should be at least 4 bytes long.", bytes.Length)
                )));
            }
            int identifier = BitConverter.ToInt32(bytes, 0);
            if (!constructors.TryGetValue(identifier, out ConstructorInfo packetConstructor)) {
                throw log.ExitError(new PacketFactoryAssignException(string.Format("Given byte[] has an identifier {0}, which has not been assigned to a type.", identifier)));
            }
            const int PREFIX_LENGTH = 4;
            byte[] packetData = new byte[bytes.Length - PREFIX_LENGTH];
            Array.Copy(bytes, PREFIX_LENGTH, packetData, 0, packetData.Length);
            Packet packet;
            try {
                packet = (Packet)packetConstructor.Invoke(new[] { packetData });
            } catch (Exception) {
                throw log.ExitError(new PacketFactoryBytesException(string.Format("Ab exception occurred trying to construct {0} from byte[]", packetConstructor.DeclaringType.Name)));
            }
            ValidateOrThrow("Received", packet);
            return packet;
        }

        public override string ToString() {
            return typeof(PacketFactory).FullName
                + ": ["
                + string.Join(", ", identifiers.Select(kvp => string.Format("{0}({1})", kvp.Key, kvp.Value)))
                + "]";
        }
    }
}

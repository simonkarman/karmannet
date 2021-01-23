using Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

/*
 * Example usage of Factory class
 * ---
 * Given that MessagePacket is a non abstract class that implements the Packet
 * interface and that Packet implements the IByteConstructable interface
 * ---
 * 
 * Factory<Packet> packetFactory = new Factory<Packet>();
 * packetFactory.Assign(10, typeof(MessagePacket));
 *
 * byte[] bytes = packetFactory.GetBytes(new MessagePacket("Hello World!"));
 * Packet packet = packetFactory.FromBytes(bytes);
 * if (packet is MessagePacket messagePacket) {
 *     Console.WriteLine(messagePacket.GetMessage());
 * }
*/
namespace Networking {
    public class Factory<T> where T : ByteConstructable {
        private readonly static Logger log = Logger.For<Factory<T>>();

        private readonly Dictionary<Type, int> identifiers = new Dictionary<Type, int>();
        private readonly Dictionary<int, ConstructorInfo> constructors = new Dictionary<int, ConstructorInfo>();

        public static Factory<T> BuildFromAllAssemblies() {
            Factory<T> factory = new Factory<T>();
            foreach (var type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes())) {
                if (typeof(T).IsAssignableFrom(type) && type.IsClass && !type.IsAbstract) {
                    factory.Assign(type.FullName.GetHashCode(), type);
                }
            }
            log.Info("Constructed a new factory of {0} from all assemblies: {1}", typeof(T).FullName, factory);
            return factory;
        }

        public void Assign(int identifier, Type type) {
            if (!type.IsClass || type.IsAbstract || !typeof(T).IsAssignableFrom(type)) {
                throw log.ExitError(new FactoryAssignException(string.Format(
                    "Cannot assign type {0} in Factory<{1}>, because (a) it is not a class, or (b) it is an abstract class, or (c) it does not derive from the {1} class.",
                    type.Name,
                    typeof(T).Name
                )));
            }
            if (constructors.TryGetValue(identifier, out ConstructorInfo existingConstructorInfo)) {
                throw log.ExitError(new FactoryAssignException(string.Format(
                    "Cannot assign type {0} to identifier {1} in Factory<{2}>, because the identifier {1} has already been assigned to type {3}.",
                    type.Name,
                    identifier,
                    typeof(T).Name,
                    existingConstructorInfo.DeclaringType.Name
                )));
            }
            ConstructorInfo constructor = type.GetConstructor(new[] { typeof(byte[]) });
            if (constructor == null) {
                throw log.ExitError(new FactoryAssignException(string.Format(
                    "Cannot assign type {0} in Factory<{1}>, because no constructor exists in {0} that takes a single parameter of type byte[].",
                    type.Name,
                    typeof(T).Name
                )));
            }
            if (identifiers.TryGetValue(type, out int existingIdentifier)) {
                throw log.ExitError(new FactoryAssignException(string.Format(
                    "Cannot assign type {0} to identifier {1} in Factory<{2}>, because the type {0} has already been assigned to identifier {3}.",
                    type.Name,
                    identifier,
                    typeof(T).Name,
                    existingIdentifier
                )));
            }
            identifiers.Add(type, identifier);
            constructors.Add(identifier, constructor);
        }

        private void ValidateOrThrow(string action, T instance) {
            bool isValid;
            try {
                isValid = instance.IsValid();
            } catch (Exception ex) {
                log.Error("An exception occured while trying to validate an instance of {0}: {1}", instance.GetType().Name, ex.Message);
                isValid = false;
            }
            if (!isValid) {
                throw log.ExitError(new ByteConstructableInstanceInvalidException(action, instance.GetType().Name));
            }
        }

        public byte[] GetBytes(T instance) {
            ValidateOrThrow("get bytes of", instance);
            Type type = instance.GetType();
            if (!identifiers.TryGetValue(type, out int identifier)) {
                throw log.ExitError(new FactoryAssignException(string.Format(
                    "Cannot get bytes for the given {0}, because the identifier corresponding to type {1} cannot be found. Ensure that you have assigned an identifier to the type using Factory<{0}>.Assign(identifer, type).",
                    typeof(T).Name,
                    type.Name
                )));
            }
            try {
                byte[] prefix = BitConverter.GetBytes(identifier);
                byte[] data = instance.GetBytes();

                int length = prefix.Length + data.Length;
                byte[] bytes = new byte[length];
                prefix.CopyTo(bytes, 0);
                data.CopyTo(bytes, prefix.Length);

                return bytes;
            } catch (Exception) {
                throw log.ExitError(new FactoryBytesException(string.Format("An exception occurred trying to get bytes of {0}", instance.GetType().Name)));
            }
        }

        public T FromBytes(byte[] bytes) {
            const int PREFIX_LENGTH = 4;
            if (bytes == null || bytes.Length < PREFIX_LENGTH) {
                throw log.ExitError(new FactoryBytesException(string.Format("Cannot construct an instance of {0} from the given bytes, because the byte array has a {1}.",
                    typeof(T).Name,
                    bytes == null
                        ? "value of null"
                        : string.Format("length of {0} byte(s), while it should be at least {1} bytes long.", bytes.Length, PREFIX_LENGTH)
                )));
            }
            int identifier = BitConverter.ToInt32(bytes, 0);
            if (!constructors.TryGetValue(identifier, out ConstructorInfo constructor)) {
                throw log.ExitError(new FactoryAssignException(string.Format("Given byte[] has an identifier {0}, which has not been assigned to a type.", identifier)));
            }
            byte[] payload = new byte[bytes.Length - PREFIX_LENGTH];
            Array.Copy(bytes, PREFIX_LENGTH, payload, 0, payload.Length);
            T instance;
            try {
                instance = (T)constructor.Invoke(new[] { payload });
            } catch (Exception) {
                throw log.ExitError(new FactoryBytesException(string.Format("Ab exception occurred trying to construct {0} from byte[]", constructor.DeclaringType.Name)));
            }
            ValidateOrThrow("construct", instance);
            return instance;
        }

        public override string ToString() {
            return "Factory<" + typeof(T).Name + ">: "
                + "["
                + string.Join(", ", identifiers.Select(kvp => string.Format("{0}({1})", kvp.Key, kvp.Value)))
                + "]";
        }
    }
}

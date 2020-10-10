using System;
using System.Collections.Generic;
using System.Text;

public class ClientInformationPacket : Networking.Packet {

    private readonly Guid clientId;
    private readonly string clientName;

    public ClientInformationPacket(byte[] bytes) : base(bytes) {
        int numberOfBytesInClientId = BitConverter.ToInt32(bytes, 0);
        byte[] bytesOfClientId = new byte[numberOfBytesInClientId];
        Array.Copy(bytes, 4, bytesOfClientId, 0, numberOfBytesInClientId);
        clientId = new Guid(bytesOfClientId);
        clientName = Encoding.ASCII.GetString(bytes, 4 + numberOfBytesInClientId, bytes.Length - 4 - numberOfBytesInClientId);
    }

    public ClientInformationPacket(Guid clientId, string clientName) : base(ToBytes(clientId, clientName)) {
        this.clientId = clientId;
        this.clientName = clientName;
    }

    private static byte[] ToBytes(Guid clientId, string clientName) {
        List<byte> bytes = new List<byte>();
        byte[] bytesOfClientId = clientId.ToByteArray();
        bytes.AddRange(BitConverter.GetBytes(bytesOfClientId.Length));
        bytes.AddRange(bytesOfClientId);
        bytes.AddRange(Encoding.ASCII.GetBytes(clientName));
        return bytes.ToArray();
    }

    public override void Validate() {
        if (clientId == Guid.Empty) {
            throw new InvalidOperationException("Cannot create a ClientInformationPacket with a clientId of Guid.Empty");
        }
        if (clientName.Length < 3) {
            throw new InvalidOperationException("Cannot create a ClientInformationPacket with a clientName of less than 3 characters");
        }
    }

    public Guid GetClientId() {
        return clientId;
    }

    public string GetClientName() {
        return clientName;
    }
}

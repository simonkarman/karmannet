using System;
using System.Collections.Generic;
using System.Text;

public class ProvideUsernamePacket : Networking.Packet {

    private readonly Guid secret;
    private readonly string username;

    public ProvideUsernamePacket(byte[] bytes) : base(bytes) {
        int numberOfBytesInSecret = BitConverter.ToInt32(bytes, 0);
        byte[] bytesOfSecret = new byte[numberOfBytesInSecret];
        Array.Copy(bytes, 4, bytesOfSecret, 0, numberOfBytesInSecret);
        secret = new Guid(bytesOfSecret);
        username = Encoding.ASCII.GetString(bytes, 4 + numberOfBytesInSecret, bytes.Length - 4 - numberOfBytesInSecret);
    }

    public ProvideUsernamePacket(Guid secret, string username) : base(ToBytes(secret, username)) {
        this.secret = secret;
        this.username = username;
    }

    private static byte[] ToBytes(Guid secret, string username) {
        List<byte> bytes = new List<byte>();
        byte[] bytesOfSecret = secret.ToByteArray();
        bytes.AddRange(BitConverter.GetBytes(bytesOfSecret.Length));
        bytes.AddRange(bytesOfSecret);
        bytes.AddRange(Encoding.ASCII.GetBytes(username));
        return bytes.ToArray();
    }

    public Guid GetSecret() {
        return secret;
    }

    public string GetUsername() {
        return username;
    }
}

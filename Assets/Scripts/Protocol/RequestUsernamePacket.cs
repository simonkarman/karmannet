using System;

public class RequestUsernamePacket : Networking.Packet {

    private readonly Guid secret;

    public RequestUsernamePacket(byte[] bytes): base(bytes) {
        secret = new Guid(bytes);
    }

    public RequestUsernamePacket(Guid secret): base(secret.ToByteArray()) {
        this.secret = secret;
    }

    public Guid GetSecret() {
        return secret;
    }
}

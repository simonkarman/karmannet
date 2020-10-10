using System.Text;

public class MessagePacket : Networking.Packet {
    private readonly string message;

    public MessagePacket(byte[] bytes) : base(bytes) {
        message = Encoding.ASCII.GetString(bytes);
    }

    public MessagePacket(string message) : base(Encoding.ASCII.GetBytes(message)) {
        this.message = message;
    }

    public override void Validate() {
        // No validation needed for the message
    }

    public string GetMessage() {
        return message;
    }
}

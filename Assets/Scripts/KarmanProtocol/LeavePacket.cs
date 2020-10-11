namespace KarmanProtocol {
    public class LeavePacket : Networking.Packet {

        public LeavePacket(byte[] bytes) : base(bytes) { }
        public LeavePacket() : base(new byte[0]) { }

        public override void Validate() { }
    }
}

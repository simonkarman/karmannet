using System;

public class PacketFramer {
    private readonly int maxPacketSize;
    private readonly Action<byte[]> onPacketCompleted;

    private readonly byte[] lengthPrefixBuffer;
    private byte[] packetBuffer = null;
    private int bytesInCurrentBuffer = 0;

    public byte[] FramePacket(byte[] packet) {
        byte[] lengthPrefix = BitConverter.GetBytes(packet.Length);
        int framedPacketSize = lengthPrefix.Length + packet.Length;
        if (framedPacketSize > maxPacketSize) {
            throw new InvalidOperationException(string.Format("Client cannot send a packet larger than {0} bytes to the server", maxPacketSize));
        }
        byte[] framedPacket = new byte[framedPacketSize];
        lengthPrefix.CopyTo(framedPacket, 0);
        packet.CopyTo(framedPacket, lengthPrefix.Length);
        return framedPacket;
    }

    public PacketFramer(int maxPacketSize, Action<byte[]> onPacketCompleted) {
        this.maxPacketSize = maxPacketSize;
        this.onPacketCompleted = onPacketCompleted;
        lengthPrefixBuffer = new byte[sizeof(int)];
    }

    public void Append(byte[] receivedData) {
        int bytesProcessed = 0;
        while (bytesProcessed != receivedData.Length) {
            byte[] currentBuffer = packetBuffer ?? lengthPrefixBuffer;

            int bytesNeeded = currentBuffer.Length - bytesInCurrentBuffer;
            int bytesLeft = receivedData.Length - bytesProcessed;
            int bytesTransferred = Math.Min(bytesNeeded, bytesLeft);
            Array.Copy(receivedData, bytesProcessed, currentBuffer, bytesInCurrentBuffer, bytesTransferred);
            bytesProcessed += bytesTransferred;

            bytesInCurrentBuffer += bytesTransferred;
            if (bytesInCurrentBuffer == currentBuffer.Length) {
                OnBufferFilled();
            } else {
                // We cannot fill the current buffer: just wait for more data to arrive
            }
        }
    }

    private void OnBufferFilled() {
        if (packetBuffer == null) {
            int packetLength = BitConverter.ToInt32(lengthPrefixBuffer, 0);
            if (packetLength < 0 || packetLength > maxPacketSize) {
                throw new InvalidOperationException(string.Format("Packet length {0} is less than zero or is larger than maximum packet size {1}", packetLength, maxPacketSize));
            }

            if (packetLength == 0) {
                onPacketCompleted(new byte[0]);
                bytesInCurrentBuffer = 0;
            } else {
                packetBuffer = new byte[packetLength];
                bytesInCurrentBuffer = 0;
            }
        } else {
            onPacketCompleted(packetBuffer);
            packetBuffer = null;
            bytesInCurrentBuffer = 0;
        }
    }
}
using System;

public class ByteFramer {
    private readonly int maxPacketSize;
    private readonly Action<byte[]> onFrameComplete;

    private readonly byte[] prefixBuffer;
    private byte[] dataBuffer = null;
    private int bytesInCurrentBuffer = 0;

    public byte[] Frame(byte[] bytes) {
        if (bytes.Length > maxPacketSize) {
            throw new InvalidOperationException(string.Format("Client cannot send a frame larger than {0} bytes to the server", maxPacketSize));
        }
        byte[] lengthPrefix = BitConverter.GetBytes(bytes.Length);
        int framedPacketSize = lengthPrefix.Length + bytes.Length;
        byte[] frame = new byte[framedPacketSize];
        lengthPrefix.CopyTo(frame, 0);
        bytes.CopyTo(frame, lengthPrefix.Length);
        return frame;
    }

    public ByteFramer(int maxPacketSize, Action<byte[]> onFrameCompleted) {
        this.maxPacketSize = maxPacketSize;
        this.onFrameComplete = onFrameCompleted;
        prefixBuffer = new byte[sizeof(int)];
    }

    public void Append(byte[] bytes) {
        int bytesProcessed = 0;
        while (bytesProcessed != bytes.Length) {
            byte[] currentBuffer = dataBuffer ?? prefixBuffer;

            int bytesNeeded = currentBuffer.Length - bytesInCurrentBuffer;
            int bytesLeft = bytes.Length - bytesProcessed;
            int bytesTransferred = Math.Min(bytesNeeded, bytesLeft);
            Array.Copy(bytes, bytesProcessed, currentBuffer, bytesInCurrentBuffer, bytesTransferred);
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
        if (dataBuffer == null) {
            int frameSize = BitConverter.ToInt32(prefixBuffer, 0);
            if (frameSize < 0 || frameSize > maxPacketSize) {
                throw new InvalidOperationException(string.Format("Frame size {0} is less than zero or is larger than maximum frame size {1}", frameSize, maxPacketSize));
            }

            if (frameSize == 0) {
                onFrameComplete(new byte[0]);
                bytesInCurrentBuffer = 0;
            } else {
                dataBuffer = new byte[frameSize];
                bytesInCurrentBuffer = 0;
            }
        } else {
            onFrameComplete(dataBuffer);
            dataBuffer = null;
            bytesInCurrentBuffer = 0;
        }
    }
}
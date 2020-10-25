using System;

namespace Networking {
    public class ByteFramer {
        private static readonly Logger log = Logger.For<ByteFramer>();

        private readonly int maxFrameSize;
        private readonly Action<byte[]> onFrameCompleted;

        private readonly byte[] prefixBuffer;
        private byte[] dataBuffer = null;
        private int bytesInCurrentBuffer = 0;

        public byte[] Frame(byte[] bytes) {
            if (bytes.Length > maxFrameSize) {
                throw log.ExitError(new InvalidFrameSizeException(bytes.Length, maxFrameSize));
            }
            byte[] lengthPrefix = BitConverter.GetBytes(bytes.Length);
            int framedPacketSize = lengthPrefix.Length + bytes.Length;
            byte[] frame = new byte[framedPacketSize];
            lengthPrefix.CopyTo(frame, 0);
            bytes.CopyTo(frame, lengthPrefix.Length);
            return frame;
        }

        public ByteFramer(int maxFrameSize, Action<byte[]> onFrameCompleted) {
            this.maxFrameSize = maxFrameSize;
            this.onFrameCompleted = onFrameCompleted;
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
                if (frameSize < 0 || frameSize > maxFrameSize) {
                    throw log.ExitError(new InvalidFrameSizeException(frameSize, maxFrameSize));
                }

                if (frameSize == 0) {
                    log.Trace("ByteFramer completed an empty frame");
                    onFrameCompleted(new byte[0]);
                    bytesInCurrentBuffer = 0;
                } else {
                    dataBuffer = new byte[frameSize];
                    bytesInCurrentBuffer = 0;
                }
            } else {
                log.Trace("ByteFramer completed a frame of {0} byte(s)", dataBuffer.Length);
                onFrameCompleted(dataBuffer);
                dataBuffer = null;
                bytesInCurrentBuffer = 0;
            }
        }
    }
}

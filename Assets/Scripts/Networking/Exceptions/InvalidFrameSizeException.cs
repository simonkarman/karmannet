using System;

namespace Networking {
    public class InvalidFrameSizeException : Exception {
        public InvalidFrameSizeException(int frameSize, int maximumFrameSize) :
            base(string.Format("Frame size {0} is less than zero or is larger than maximum frame size {1}", frameSize, maximumFrameSize)) {
        }
    }
}
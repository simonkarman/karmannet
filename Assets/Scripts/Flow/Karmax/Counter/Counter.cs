using KarmanNet.Karmax;
using KarmanNet.Networking;

namespace KarmaxCounter {
    public class Counter : Fragment {
        private readonly int value;

        public Counter(byte[] bytes) : base(bytes) {
            value = ReadInt();
        }

        public Counter(int value) : base(Bytes.Of(value)) {
            this.value = value;
        }

        public override bool IsValid() {
            return true;
        }

        public int GetValue() {
            return value;
        }

        public override string ToString() {
            return $"CounterFragment[value={value}]";
        }
    }
}
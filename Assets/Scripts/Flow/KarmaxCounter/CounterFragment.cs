using KarmanProtocol.Karmax;
using Networking;

namespace KarmaxExample {
    public class CounterFragment : Fragment {
        private readonly int value;

        public CounterFragment(byte[] bytes) : base(bytes) {
            value = ReadInt();
        }

        public CounterFragment(int value) : base(Bytes.Of(value)) {
            this.value = value;
        }

        public static CounterFragment Identity() {
            return new CounterFragment(0);
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
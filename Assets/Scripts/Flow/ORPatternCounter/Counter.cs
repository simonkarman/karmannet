namespace ORPatternExample {
    public class Counter {
        private readonly int value;

        public Counter(int value) {
            this.value = value;
        }

        public int GetValue() {
            return value;
        }

        public override string ToString() {
            return $"Counter[value={value}]";
        }
    }
}

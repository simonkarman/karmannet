using Networking;

public class Increment : CounterFragmentMutation {
    private readonly int amount;

    public Increment(byte[] bytes) : base(bytes) {
        amount = Bytes.GetInt32(bytes);
    }

    private Increment(int amount) : base(Bytes.Of(amount)) {
        this.amount = amount;
    }

    public static Increment By(int amount) {
        return new Increment(amount);
    }

    public int GetAmount() {
        return amount;
    }

    public override bool IsValid() {
        return true;
    }

    public override CounterFragment ApplyOn(CounterFragment counterFragment) {
        return new CounterFragment(counterFragment.GetValue() + amount);
    }
}

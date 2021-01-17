using Networking;

public class Multiply : CounterFragmentMutation {
    private readonly int product;

    public Multiply(byte[] bytes) : base(bytes) {
        product = Bytes.GetInt32(bytes);
    }

    private Multiply(int product) : base(Bytes.Of(product)) {
        this.product = product;
    }

    public static Multiply By(int product) {
        return new Multiply(product);
    }

    public int GetProduct() {
        return product;
    }

    public override bool IsValid() {
        return true;
    }

    public override CounterFragment ApplyOn(CounterFragment counterFragment) {
        return new CounterFragment(counterFragment.GetValue() * product);
    }
}

namespace KarmanNet.Networking {
    public class FactoryAssignException : NetworkingException {
        public FactoryAssignException(string reason):
            base(reason) {
        }
    }
}
namespace KarmanNet.Networking {
    public class InvalidConnectionStringException : NetworkingException {
        public InvalidConnectionStringException(string connectionString) :
            base(string.Format("Connection string {0} is in an invalid format", connectionString)) {
        }
    }
}
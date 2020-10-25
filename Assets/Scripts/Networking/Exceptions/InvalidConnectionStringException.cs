using System;

namespace Networking {
    public class InvalidConnectionStringException : Exception {
        public InvalidConnectionStringException(string connectionString) :
            base(string.Format("Connection string {0} is in an invalid format", connectionString)) {
        }
    }
}
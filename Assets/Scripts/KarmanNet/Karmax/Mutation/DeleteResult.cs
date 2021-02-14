using System;

namespace KarmanNet.Karmax {
    public class DeleteResult : MutationResult {
        private DeleteResult(string failureReason) : base(null, failureReason) { }

        public new static DeleteResult Failure(string reason) {
            if (reason == null) {
                throw new ArgumentNullException("reason");
            }
            return new DeleteResult(reason);
        }

        public new static DeleteResult Delete() {
            return new DeleteResult(null);
        }
    }
}
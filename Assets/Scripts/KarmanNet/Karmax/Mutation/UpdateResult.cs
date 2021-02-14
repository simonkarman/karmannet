using System;

namespace KarmanNet.Karmax {
    public class UpdateResult<T> : MutationResult where T : Fragment {
        private UpdateResult(Fragment fragment, string failureReason) : base(fragment, failureReason) { }

        public static UpdateResult<T> Success(T fragment) {
            if (fragment == null) {
                throw new ArgumentNullException("fragment");
            }
            return new UpdateResult<T>(fragment, null);
        }

        public new static UpdateResult<T> Failure(string reason) {
            if (reason == null) {
                throw new ArgumentNullException("reason");
            }
            return new UpdateResult<T>(null, reason);
        }

        public new static UpdateResult<T> Delete() {
            return new UpdateResult<T>(null, null);
        }
    }
}
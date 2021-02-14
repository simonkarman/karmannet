using System;

namespace KarmanNet.Karmax {
    public class InsertResult<T> : MutationResult where T : Fragment {
        private InsertResult(Fragment fragment, string failureReason) : base(fragment, failureReason) { }

        public static InsertResult<T> Success(T fragment) {
            if (fragment == null) {
                throw new ArgumentNullException("fragment");
            }
            return new InsertResult<T>(fragment, null);
        }

        public new static InsertResult<T> Failure(string reason) {
            if (reason == null) {
                throw new ArgumentNullException("reason");
            }
            return new InsertResult<T>(null, reason);
        }
    }
}
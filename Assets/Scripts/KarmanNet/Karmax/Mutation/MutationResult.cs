using System;

namespace KarmanNet.Karmax { 
    public class MutationResult {
        private const string prefix = "karmax/";
        public static readonly MutationResult ImposterFailure = Failure($"{prefix}imposter");
        public static readonly MutationResult ResultNullFailure = Failure($"{prefix}result-null");
        public static readonly MutationResult FragmentNotFoundFailure = Failure($"{prefix}fragment/not-found");
        public static readonly MutationResult FragmentAlreadyExistsFailure = Failure($"{prefix}fragment/already-exists");
        public static readonly MutationResult FragmentTypeMismatchFailure = Failure($"{prefix}fragment/type-mismatch");

        private readonly Fragment fragment;
        private readonly string failureReason;

        protected MutationResult(Fragment fragment, string failureReason) {
            this.fragment = fragment;
            this.failureReason = failureReason;
        }

        public static MutationResult Failure(string reason) {
            if (reason == null) {
                throw new ArgumentNullException("reason");
            }
            return new MutationResult(null, reason);
        }

        public static MutationResult Delete() {
            return new MutationResult(null, null);
        }

        public bool IsSuccess() {
            return fragment != null;
        }

        public Fragment GetFragment() {
            return fragment;
        }

        public bool IsFailure() {
            return failureReason != null;
        }

        public string GetFailureReason() {
            return failureReason;
        }

        public bool IsDelete() {
            return fragment == null && failureReason == null;
        }
    }
}
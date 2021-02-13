namespace KarmanNet.Karmax {
    public static class MutationFailureReason {
        private const string prefix = "karmax::";
        public static readonly string Imposter = $"{prefix}imposter";
        public static readonly string FragmentNotFound = $"{prefix}fragment/not-found";
        public static readonly string FragmentAlreadyExists = $"{prefix}fragment/already-exists";
        public static readonly string FragmentNull = $"{prefix}fragment/null";
        public static readonly string FragmentTypeMismatch = $"{prefix}fragment/type-mismatch";
    }
}
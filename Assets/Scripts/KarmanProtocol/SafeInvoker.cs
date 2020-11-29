using System;
using Logging;
using Networking;

namespace KarmanProtocol {
    public class SafeInvoker {
        private static void LogExceptionError(Logger log, string name, Exception ex) {
            log.Error("An invocation of the {0} has thrown a(n) {1}: {2}", name, ex.GetType().Name, ex.Message);
        }

        public static bool Invoke(Logger log, string name, Action action) {
            bool allSucceeded = true;
            if (action != null) {
                foreach (Delegate invocation in action.GetInvocationList()) {
                    try {
                        (invocation as Action)?.Invoke();
                    } catch (Exception ex) {
                        allSucceeded = false;
                        LogExceptionError(log, name, ex);
                    }
                }
            }
            return allSucceeded;
        }

        public static bool Invoke<T>(Logger log, string name, Action<T> action, T obj) {
            bool allSucceeded = true;
            if (action != null) {
                foreach (Delegate invocation in action.GetInvocationList()) {
                    try {
                        (invocation as Action<T>)?.Invoke(obj);
                    } catch (Exception ex) {
                        allSucceeded = false;
                        LogExceptionError(log, name, ex);
                    }
                }
            }
            return allSucceeded;
        }

        public static bool Invoke<T1, T2>(Logger log, string name, Action<T1, T2> action, T1 obj1, T2 obj2) {
            bool allSucceeded = true;
            if (action != null) {
                foreach (Delegate invocation in action.GetInvocationList()) {
                    try {
                        (invocation as Action<T1, T2>)?.Invoke(obj1, obj2);
                    } catch (Exception ex) {
                        allSucceeded = false;
                        LogExceptionError(log, name, ex);
                    }
                }
            }
            return allSucceeded;
        }

        public static bool Invoke<T1, T2, T3>(Logger log, string name, Action<T1, T2, T3> action, T1 obj1, T2 obj2, T3 obj3) {
            bool allSucceeded = true;
            if (action != null) {
                foreach (Delegate invocation in action.GetInvocationList()) {
                    try {
                        (invocation as Action<T1, T2, T3>)?.Invoke(obj1, obj2, obj3);
                    } catch (Exception ex) {
                        allSucceeded = false;
                        LogExceptionError(log, name, ex);
                    }
                }
            }
            return allSucceeded;
        }
    }
}

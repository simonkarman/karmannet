using System;
using System.Collections.Generic;
using UnityEngine;

namespace KarmanNet.Networking {
    public class ThreadManager : MonoBehaviour {
        private static readonly KarmanNet.Logging.Logger log = KarmanNet.Logging.Logger.For<ThreadManager>();

        private static ThreadManager instance;
        private readonly List<Action> requestedActions = new List<Action>();
        private readonly List<Action> currentActions = new List<Action>();

        public static void Activate() {
            if (!instance) {
                log.Trace("Thread Manager activated");
                instance = new GameObject("Thread Manager").AddComponent<ThreadManager>();
            }
        }

        public static void ExecuteOnMainThread(Action _action) {
            lock (instance.requestedActions) {
                instance.requestedActions.Add(_action);
            }
        }

        protected void Update() {
            lock (requestedActions) {
                currentActions.AddRange(requestedActions);
                requestedActions.Clear();
            }
            for (int i = 0; i < currentActions.Count; i++) {
                try {
                    currentActions[i]();
                } catch (Exception ex) {
                    log.Error("An error occurred, while executing an action in the thread manager: {0}", ex);
                }
            }
            currentActions.Clear();
        }
    }
}

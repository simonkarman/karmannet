using KarmanProtocol.Karmax;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace KarmaxExample {
    public class KarmaxOracleWrapper : MonoBehaviour {
        private readonly static Logging.Logger log = Logging.Logger.For<KarmaxOracleWrapper>();

        [SerializeField]
        private ServerFlow serverFlow = default;
        [SerializeField]
        private Text stateRepresentation = default;

        protected IEnumerator Start() {
            Oracle oracle = new Oracle(serverFlow.GetKarmanServer());
            serverFlow.GetKarmanServer().OnClientJoinedCallback += (Guid clientId, string clientName) => { oracle.Request($"{clientId}::joined", Increment.By(1)); };
            serverFlow.GetKarmanServer().OnClientConnectedCallback += (Guid clientId) => { oracle.Request($"{clientId}::connected", Increment.By(1)); };
            serverFlow.GetKarmanServer().OnClientDisconnectedCallback += (Guid clientId) => { oracle.Request($"{clientId}::connected", Increment.By(-1)); };
            serverFlow.GetKarmanServer().OnClientLeftCallback += (Guid clientId, string reason) => { oracle.Request($"{clientId}::joined", Increment.By(-1)); };
            log.Info("Started!");
            oracle.OnMutatedCallback += OnMutated;
            yield return new WaitForSeconds(5f);
            while (true) {
                yield return new WaitForSeconds(1f);
                oracle.Request("counter", Increment.By(1));
            }
        }

        private void OnMutated(IReadOnlyDictionary<string, Fragment> state, string fragmentId, Mutation mutation) {
            stateRepresentation.text = string.Join("\n", state.Select(kvp => $"<b>{kvp.Key}</b>: {kvp.Value}").Reverse());
        }
    }
}

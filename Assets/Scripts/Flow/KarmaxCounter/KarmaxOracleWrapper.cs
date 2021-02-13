using KarmanProtocol.Karmax;
using System;
using System.Collections;
using UnityEngine;

namespace KarmaxExample {
    public class KarmaxOracleWrapper : KarmaxWrapper {
        [SerializeField]
        private ServerFlow serverFlow = default;

        protected override Container BuildContainer() {
            return new Oracle(serverFlow.GetKarmanServer());
        }

        protected override IEnumerator Start() {
            serverFlow.GetKarmanServer().OnClientJoinedCallback += (Guid clientId, string clientName) => { container.Request($"client/{clientId}/joined", Increment.By(1)); };
            serverFlow.GetKarmanServer().OnClientConnectedCallback += (Guid clientId) => { container.Request($"client/{clientId}/connected", Increment.By(1)); };
            serverFlow.GetKarmanServer().OnClientDisconnectedCallback += (Guid clientId) => { container.Request($"client/{clientId}/connected", Increment.By(-1)); };
            serverFlow.GetKarmanServer().OnClientLeftCallback += (Guid clientId, string reason) => { container.Request($"client/{clientId}/joined", Increment.By(-1)); };
            yield return StartCoroutine(base.Start());
        }

        protected override string GetFragmentName() {
            return "server";
        }
    }
}

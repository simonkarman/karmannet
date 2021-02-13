using KarmanNet.Karmax;
using System;
using System.Collections;
using UnityEngine;

namespace KarmaxCounter {
    public class KarmaxOracleWrapper : KarmaxWrapper {
        [SerializeField]
        private ServerFlow serverFlow = default;

        protected override Container BuildContainer() {
            return new Oracle(serverFlow.GetKarmanServer());
        }

        protected override IEnumerator Start() {
            var karmanServer = serverFlow.GetKarmanServer();
            karmanServer.OnClientJoinedCallback += (Guid clientId, string clientName) => {
                container.Request($"client/{clientId}", Set.To(0));
            };
            karmanServer.OnClientConnectedCallback += (Guid clientId) => {
                container.Request($"client/{clientId}", Increment.By(1));
            };
            karmanServer.OnClientDisconnectedCallback += (Guid clientId) => {
                container.Request($"client/{clientId}", Increment.By(-1));
            };
            karmanServer.OnClientLeftCallback += (Guid clientId, string reason) => {
                container.Request($"client/{clientId}/counter", new Mutation.Delete());
                container.Request($"client/{clientId}", new Mutation.Delete());
            };
            yield return StartCoroutine(base.Start());
        }

        protected override string GetFragmentName() {
            return "server";
        }
    }
}

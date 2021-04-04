using KarmanNet.Karmax;
using KarmanNet.Networking;
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
                container.Request(new ConnectionKey(clientId), Set.To(0));
            };
            karmanServer.OnClientConnectedCallback += (Guid clientId) => {
                container.Request(new ConnectionKey(clientId), Increment.By(1));
            };
            karmanServer.OnClientDisconnectedCallback += (Guid clientId) => {
                container.Request(new ConnectionKey(clientId), Increment.By(-1));
            };
            karmanServer.OnClientLeftCallback += (Guid clientId, string reason) => {
                container.Request(new ScoreKey(clientId), new Clear());
                container.Request(new ConnectionKey(clientId), new Clear());
            };
            yield return StartCoroutine(base.Start());
        }
    }
}

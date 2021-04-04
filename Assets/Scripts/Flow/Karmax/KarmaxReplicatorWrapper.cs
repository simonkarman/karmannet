using UnityEngine;
using KarmanNet.Karmax;
using KarmanNet.Networking;

namespace KarmaxCounter {
    public class KarmaxReplicatorWrapper : KarmaxWrapper {
        [SerializeField]
        private ClientFlow clientFlow = default;

        protected override Container BuildContainer() {
            return new Replicator(clientFlow.GetKarmanClient());
        }
    }
}

using UnityEngine;
using KarmanNet.Karmax;

namespace KarmaxCounter {
    public class KarmaxReplicatorWrapper : KarmaxWrapper {
        [SerializeField]
        private ClientFlow clientFlow = default;

        protected override Container BuildContainer() {
            return new Replicator(clientFlow.GetKarmanClient());
        }

        protected override string GetFragmentName() {
            return $"client/{container.containerId}/counter";
        }
    }
}

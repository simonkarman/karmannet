using UnityEngine;
using KarmanProtocol.Karmax;

namespace KarmaxExample {
    public class KarmaxReplicatorWrapper : KarmaxWrapper {
        [SerializeField]
        private ClientFlow clientFlow = default;

        protected override Container BuildContainer() {
            return new Replicator(clientFlow.GetKarmanClient());
        }

        protected override string GetFragmentName() {
            return $"client/${container.id}";
        }
    }
}

using KarmanProtocol.Karmax;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace KarmaxExample {
    public abstract class KarmaxWrapper : MonoBehaviour {
        private readonly static Logging.Logger log = Logging.Logger.For<KarmaxWrapper>();

        protected Container container;

        [SerializeField]
        private Text stateRepresentation = default;

        protected virtual IEnumerator Start() {
            container = BuildContainer();
            container.OnMutatedCallback += OnMutated;
            log.Info("Started!");
            yield return new WaitForSeconds(5f);
            while (true) {
                yield return new WaitForSeconds(1f);
                container.Request(GetFragmentName(), Increment.By(1));
            }
        }

        protected abstract Container BuildContainer();
        protected abstract string GetFragmentName();

        private void OnMutated(IReadOnlyDictionary<string, Fragment> state, string fragmentId, Mutation mutation) {
            stateRepresentation.text = string.Join("\n", state.Select(kvp => $"<b>{kvp.Key}</b>: {kvp.Value}").Reverse());
        }
    }
}
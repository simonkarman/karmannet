using KarmanNet.Karmax;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace KarmaxCounter {
    public abstract class KarmaxWrapper : MonoBehaviour {
        private readonly static KarmanNet.Logging.Logger log = KarmanNet.Logging.Logger.For<KarmaxWrapper>();

        protected Container container;
        private string lastStateText = "none";
        private string lastFailedText = "";

        [SerializeField]
        private Text stateRepresentation = default;

        protected virtual IEnumerator Start() {
            container = BuildContainer();
            container.OnStateChangedCallback += OnStateChanged;
            container.OnMutationFailedCallback += OnMutationFailed;
            log.Info("Started!");
            yield return new WaitForSeconds(5f);
            while (true) {
                yield return new WaitForSeconds(1f);
                container.Request(GetFragmentName(), IncrementOrSet.At(3));
            }
        }

        protected abstract Container BuildContainer();
        protected abstract string GetFragmentName();

        private void OnStateChanged(IReadOnlyDictionary<string, Fragment> state, string fragmentId, Mutation mutation) {
            lastStateText = string.Join("\n", state.Select(kvp => $"<b>{kvp.Key}</b>: {kvp.Value}").Reverse());
            UpdateText();
        }

        private void OnMutationFailed(Guid id, string failureReason) {
            lastFailedText = $"Last failure ({DateTime.Now:yyyy-MM-dd HH:mm:ss}): ${failureReason} ({id})\n\n";
            UpdateText();
        }

        private void UpdateText() {
            stateRepresentation.text = $"{lastFailedText}State:\n{lastStateText}";
        }
    }
}
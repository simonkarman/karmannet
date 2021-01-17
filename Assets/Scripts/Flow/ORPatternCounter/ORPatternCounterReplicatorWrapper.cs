using UnityEngine;
using KarmanProtocol.ORPattern;
using System;
using System.Collections;
using UnityEngine.UI;

namespace ORPatternExample {
    public class ORPatternCounterReplicatorWrapper : MonoBehaviour {
        [SerializeField]
        private ClientFlow clientFlow;
        [SerializeField]
        private Text stateRepresentation;

        protected IEnumerator Start() {
            var replicator = new Replicator<CounterState, Counter>(clientFlow.GetKarmanClient(), new CounterState(0));
            replicator.OnStateInitialized += (Counter state) => {
                stateRepresentation.text = state.ToString();
            };
            replicator.OnStateChanged += (Counter state, Counter oldState, StateChangedEvent<Counter> stateChangedEvent) => {
                stateRepresentation.text = state.ToString();
            };
            yield return new WaitForSeconds(5f);
            while (true) {
                replicator.RequestStateChange(new Increment(Guid.Empty, 1));
                yield return new WaitForSeconds(1f);
            }
        }
    }
}
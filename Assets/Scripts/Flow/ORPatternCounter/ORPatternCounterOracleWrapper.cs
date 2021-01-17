using UnityEngine;
using KarmanProtocol.ORPattern;
using System;
using System.Collections;
using UnityEngine.UI;

namespace ORPatternExample {
    public class ORPatternCounterOracleWrapper : MonoBehaviour {
        [SerializeField]
        private ServerFlow serverFlow;
        [SerializeField]
        private Text stateRepresentation;

        protected IEnumerator Start() {
            var oracle = new Oracle<CounterState, Counter>(serverFlow.GetKarmanServer(), new CounterState(0));
            oracle.OnStateInitialized += (Counter state) => {
                stateRepresentation.text = state.ToString();
            };
            oracle.OnStateChanged += (Counter state, Counter oldState, StateChangedEvent<Counter> stateChangedEvent) => {
                stateRepresentation.text = state.ToString();
            };
            yield return new WaitForSeconds(5f);
            while (true) {
                for (int i = 0; i < 10; i++) {
                    oracle.RequestStateChange(new Increment(Guid.Empty, 1));
                    yield return new WaitForSeconds(1f);
                }
                oracle.RequestStateChange(new Multiply(Guid.Empty, 2));
                yield return new WaitForSeconds(1f);
            }
        }
    }
}
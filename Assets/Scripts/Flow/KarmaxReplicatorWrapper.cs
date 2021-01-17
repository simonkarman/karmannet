using UnityEngine;
using UnityEngine.UI;
using KarmanProtocol.Karmax;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class KarmaxReplicatorWrapper : MonoBehaviour {
    private readonly static Logging.Logger log = Logging.Logger.For<KarmaxReplicatorWrapper>();

    [SerializeField]
    private ClientFlow clientFlow = default;
    [SerializeField]
    private Text stateRepresentation = default;

    protected IEnumerator Start() {
        Replicator replicator = new Replicator(clientFlow.GetKarmanClient());
        log.Info("Started!");
        replicator.OnMutatedCallback += OnMutated;
        yield return new WaitForSeconds(5f);
        while (true) {
            yield return new WaitForSeconds(1f);
            replicator.Request("counter", Increment.By(1));
        }
    }

    private void OnMutated(IReadOnlyDictionary<string, Fragment> state, string fragmentId, Mutation mutation) {
        stateRepresentation.text = string.Join("\n", state.Select(kvp => $"<b>{kvp.Key}</b>: {kvp.Value}").Reverse());
    }
}

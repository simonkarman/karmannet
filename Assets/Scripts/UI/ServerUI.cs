using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ServerUI : MonoBehaviour {
    [SerializeField]
    private Transform playerUiParent = default;
    [SerializeField]
    private GameObject playerUiPrefab = default;
    [SerializeField]
    private ServerFlow serverFlow = default;
    [SerializeField]
    private Text serverStatusText = default;

    protected void Start() {
        serverFlow.onPlayersChanged += OnPlayersChanged;
    }

    private void OnPlayersChanged(IReadOnlyList<ServerFlow.ServerFlowPlayer> flowPlayers) {
        ServerPlayerUI[] playerUIs = playerUiParent.GetComponentsInChildren<ServerPlayerUI>(true);
        for (int i = playerUIs.Length - 1; i >= flowPlayers.Count; i--) {
            playerUIs[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < flowPlayers.Count; i++) {
            ServerPlayerUI playerUI;
            if (playerUIs.Length <= i) {
                playerUI = Instantiate(playerUiPrefab, playerUiParent).GetComponent<ServerPlayerUI>();
                playerUI.transform.SetAsLastSibling();
            } else {
                playerUI = playerUIs[i];
                playerUI.gameObject.SetActive(true);
            }
            playerUI.SetFrom(flowPlayers[i]);
        }
    }

    protected void Update() {
        serverStatusText.text = serverFlow.GetServerStatus().ToString();
    }

    public void ScheduleShutdown() {
        serverFlow.ScheduleShutdown();
    }
}


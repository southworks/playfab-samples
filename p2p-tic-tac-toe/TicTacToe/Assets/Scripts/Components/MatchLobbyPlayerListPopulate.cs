using System.Collections.Generic;
using TicTacToe;
using TicTacToe.Models;
using UnityEngine;
using UnityEngine.UI;

public class MatchLobbyPlayerListPopulate : MonoBehaviour
{
    public GameObject prefabContainer;
    public MatchLobbyScene matchLobbyScene;

    private void Awake()
    {
        matchLobbyScene = FindObjectOfType(typeof(MatchLobbyScene)) as MatchLobbyScene;
    }

    // Update is called once per frame
    void Update()
    {
        if (ApplicationModel.MatchLobbyJoinedPlayerListHasChanged
            && (ApplicationModel.RemotePlayersHandler?.players?.Count ?? 0) >= 0)
        {
            RenderList(ApplicationModel.RemotePlayersHandler.players);
        }
    }

    private MatchLobbyScene EnsureGetMatchLobbyScene()
    {
        if (matchLobbyScene == null)
        {
            matchLobbyScene = FindObjectOfType(typeof(MatchLobbyScene)) as MatchLobbyScene;
        }

        return matchLobbyScene;
    }

    public void RenderList(List<RemotePlayer> remotePlayers)
    {
        ApplicationModel.MatchLobbyJoinedPlayerListHasChanged = false;
        ClearListContent();

        foreach (var remotePlayer in remotePlayers)
        {
            var itemList = Instantiate(prefabContainer, transform);
            var newText = itemList.GetComponentInChildren<Text>();
            newText.text = remotePlayer.playFabPlayer.EntityKey.Id;

            var newBtn = itemList.GetComponentInChildren<Button>();
            newBtn.gameObject.SetActive(ApplicationModel.IsHost);

            if (ApplicationModel.IsHost)
            {
                newBtn.onClick.AddListener(delegate { EnsureGetMatchLobbyScene().OnKickButtonClick(remotePlayer.playFabPlayer.EntityKey.Id); });
            }
        }
    }

    private void ClearListContent()
    {
        Transform panelTransform = GameObject.Find(Constants.MATCH_LOBBY_PLAYERS_LIST_CONTENT_CONTAINER_NAME).transform;
        foreach (Transform child in panelTransform)
        {
            Destroy(child.gameObject);
        }
    }
}

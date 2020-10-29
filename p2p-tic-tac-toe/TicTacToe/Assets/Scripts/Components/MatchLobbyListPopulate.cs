using System.Collections.Generic;
using TicTacToe.Models;
using UnityEngine;
using UnityEngine.UI;

public class MatchLobbyListPopulate : MonoBehaviour
{
    public GameObject prefabContainer;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (ApplicationModel.MatchLobbyList != null && ApplicationModel.MatchLobbyListHasChanged)
        {
            RenderList(ApplicationModel.MatchLobbyList);
        }
    }

    private void RenderList(List<MatchLobby> MatchLobbyList)
    {
        ApplicationModel.MatchLobbyListHasChanged = false;
        ClearMatchLobbyListContent();

        foreach (var mLobby in MatchLobbyList)
        {
            var itemList = Instantiate(prefabContainer, transform);

            var newText = itemList.GetComponentInChildren<Text>();
            newText.text = mLobby.matchLobbyId;

            var newBtn = itemList.GetComponentInChildren<Button>();
            newBtn.onClick.AddListener(delegate { onClickConnect(mLobby); });
        }
    }

    private void ClearMatchLobbyListContent()
    {
        Transform panelTransform = GameObject.Find(Constants.MATCH_LOBBY_LIST_CONTENT_CONTAINER_NAME).transform;
        foreach (Transform child in panelTransform)
        {
            Destroy(child.gameObject);
        }
    }

    private void onClickConnect(MatchLobby matchLobby)
    {
        Debug.Log($"Joining to: {JsonUtility.ToJson(matchLobby)}");
        ApplicationModel.CurrentMatchLobbyToJoin = matchLobby;
        ApplicationModel.JoinToMatchLobby = true;
    }
}

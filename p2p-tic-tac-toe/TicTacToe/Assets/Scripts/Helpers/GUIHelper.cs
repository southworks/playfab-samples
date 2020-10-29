using Assets.Scripts.Models.Responses;
using UnityEngine.UI;

namespace Assets.Scripts.Helpers
{
    public class GUIHelper
    {
        public static void UpdateGUIMessageWithStatusCode(Text textComponent, StatusCode statusCode)
        {
            switch (statusCode)
            {
                case StatusCode.OK:
                    UpdateGUIMessage(textComponent, Constants.MATCH_LOBBY_JOIN_OK);
                    break;
                case StatusCode.InternalServerError:
                    UpdateGUIMessage(textComponent, Constants.MATCH_LOBBY_JOIN_INTERNAL_SERVER_ERROR);
                    break;
                case StatusCode.NotFound:
                    UpdateGUIMessage(textComponent, Constants.MATCH_LOBBY_JOIN_ERROR_LOBBY_NOT_FOUND);
                    break;
                case StatusCode.LobbyFull:
                    UpdateGUIMessage(textComponent, Constants.MATCH_LOBBY_JOIN_ERROR_LOBBY_FULL);
                    break;
                case StatusCode.RequesterIsLobbyCreator:
                    UpdateGUIMessage(textComponent, Constants.MATCH_LOBBY_JOIN_ERROR_REQUESTER_IS_LOBBY_CREATOR);
                    break;
                case StatusCode.NotInvitationCodeIncluded:
                    UpdateGUIMessage(textComponent, Constants.MATCH_LOBBY_JOIN_ERROR_NOT_INVITATION_CODE_INCLUDED);
                    break;
                case StatusCode.InvalidInvitationCode:
                    UpdateGUIMessage(textComponent, Constants.MATCH_LOBBY_JOIN_ERROR_INVALID_INVITATION_CODE);
                    break;
                case StatusCode.OnlyLobbyCreatorCanLock:
                    UpdateGUIMessage(textComponent, Constants.SET_MATCH_LOBBY_LOCK_STATE_ONLY_LOBBY_CREATOR_CAN_LOCK);
                    break;
            }
        }

        public static void UpdateGUIMessage(Text textComponent, string message)
        {
            textComponent.GetComponent<Text>().enabled = true;
            textComponent.text = message;
        }
    }
}

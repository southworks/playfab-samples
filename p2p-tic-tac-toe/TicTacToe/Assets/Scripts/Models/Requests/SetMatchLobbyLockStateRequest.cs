using System;

namespace Assets.Scripts.Models.Requests
{
    [Serializable]
    public class SetMatchLobbyLockStateRequest
    {
        public string MatchLobbyId;

        public bool Locked;
    }
}

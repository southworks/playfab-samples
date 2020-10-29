using System;
using TicTacToe.Models.Helpers;

namespace TicTacToe.Models
{
    [Serializable]
    public class PartyNetworkMessageWrapper<T>
    {
        public PartyNetworkMessageEnum MessageType;

        public T MessageData;
    }
}

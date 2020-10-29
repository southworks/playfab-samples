using System;
using PlayFab.Party;
using TicTacToe.Helpers;
using TicTacToe.Models;
using TicTacToe.Models.Helpers;
using TicTacToe.Models.Requests;

namespace TicTacToe.Handlers
{
    public class GameStateHandler
    {
        public PlayFabMultiplayerManager Manager => PlayFabMultiplayerManager.Get();

        /// <summary>
        /// Event triggered for the reception of a GameState.
        /// </summary>
        /// <param name="gameState">The GameState received.</param> 
        public delegate void OnGameStateReceivedHandler(GameState gameState);
        public event OnGameStateReceivedHandler OnGameStateReceived;

        /// <summary>
        /// Event triggered for the reception of a GameState.
        /// </summary>
        /// <param name="move">The GameState received.</param> 
        public delegate void OnMoveReceivedHandler(string playerId, TicTacToeMove move);
        public event OnMoveReceivedHandler OnMoveReceived;

        /// <summary>
        /// Event triggered for the reception of a GameState.
        /// </summary>
        public delegate void OnMatchAbandonmentHandler();
        public event OnMatchAbandonmentHandler OnMatchAbandonment;

        public GameStateHandler()
        {
            Manager.OnDataMessageNoCopyReceived += Manager_OnDataMessageNoCopyReceived;
        }

        private void Manager_OnDataMessageNoCopyReceived(object sender, PlayFabPlayer from, IntPtr buffer, uint bufferSize)
        {
            var messageType = PartyNetworkMessageHelper.GetTypeFromMessageWrapper(buffer, bufferSize);

            switch (messageType)
            {
                case PartyNetworkMessageEnum.GameState:
                    OnGameStateReceived?.Invoke(PartyNetworkMessageHelper.GetDataFromMessageWrapper<GameState>(buffer, bufferSize));
                    break;
                case PartyNetworkMessageEnum.Move:
                    OnMoveReceived?.Invoke(from.EntityKey.Id, PartyNetworkMessageHelper.GetDataFromMessageWrapper<TicTacToeMove>(buffer, bufferSize));
                    break;
                case PartyNetworkMessageEnum.MatchAbandonment:
                    OnMatchAbandonment?.Invoke();
                    break;
            }
        }

        private void SendDataMessage<T>(T message)
        {
            PartyNetworkMessageHelper.BufferData(message, out byte[] buffer, out IntPtr unmanagedPointer);
            Manager.SendDataMessage(unmanagedPointer, (uint)buffer.Length, PlayFabMultiplayerManager.Get().RemotePlayers, DeliveryOption.BestEffort);
        }

        public void SendGameState(GameState gameState)
        {
            var message = new PartyNetworkMessageWrapper<GameState>
            {
                MessageType = PartyNetworkMessageEnum.GameState,
                MessageData = gameState
            };
            SendDataMessage(message);
        }

        public void SendMatchAbandonment()
        {
            var message = new PartyNetworkMessageWrapper<GameWinnerType>
            {
                MessageType = PartyNetworkMessageEnum.MatchAbandonment
            };
            SendDataMessage(message);
        }

        public void SendMove(TicTacToeMove move)
        {
            var message = new PartyNetworkMessageWrapper<TicTacToeMove>
            {
                MessageType = PartyNetworkMessageEnum.Move,
                MessageData = move
            };
            SendDataMessage(message);
        }

        public void Leave()
        {
            Manager.LeaveNetwork();
        }
    }
}

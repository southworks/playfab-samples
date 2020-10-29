using System;
using System.Collections;
using System.Collections.Generic;
using PlayFab.Party;
using TicTacToe.Helpers;
using TicTacToe.Models;

namespace TicTacToe.Handlers
{
    public class PartyNetworkHandler : RequestHandler
    {
        private const string NETWORK_ID_INVITE_AND_DESCRIPTOR_SEPERATOR = "|";
        private PlayFabMultiplayerManager Manager => PlayFabMultiplayerManager.Get();
        public string NetworkId;
        public bool isNetworkCreator;

        /// <summary>
        /// Event triggered by the reception of a data message.
        /// </summary>
        public event PlayFabMultiplayerManager.OnDataMessageReceivedNoCopyHandler OnDataMessageNoCopyReceived
        {
            add { Manager.OnDataMessageNoCopyReceived += value; }
            remove { Manager.OnDataMessageNoCopyReceived -= value; }
        }

        public IList<PlayFabPlayer> RemotePlayers
        {
            get => Manager.RemotePlayers;
        }

        public string InvitationId
        {
            get
            {
                int indexOfSeperator = string.IsNullOrEmpty(Manager.NetworkId) ? -1 : Manager.NetworkId.IndexOf(NETWORK_ID_INVITE_AND_DESCRIPTOR_SEPERATOR);
                return indexOfSeperator != -1 ? Manager.NetworkId.Substring(0, indexOfSeperator) : "";
            }
        }

        public PartyNetworkHandler()
        {
            AddOnNetworkJoinedListener(OnNetworkJoined);
            // Make sure that the PlayFab Party SDK is '1.5.0.1-main.0' or higher 
            AddOnNetworkLeftListener(OnNetworkLeft);
        }

        public IEnumerator CreateAndJoinToNetwork()
        {
            ExecutionCompleted = false;
            NetworkId = null;
            Manager.CreateAndJoinNetwork();
            Manager.LocalPlayer.IsMuted = true;
            isNetworkCreator = true;

            yield return WaitForExecution();
        }

        public IEnumerator JoinNetwork(string networkId)
        {
            ExecutionCompleted = false;
            NetworkId = null;
            Manager.JoinNetwork(networkId);
            Manager.LocalPlayer.IsMuted = true;
            isNetworkCreator = false;

            yield return WaitForExecution();
        }

        public IEnumerator LeaveNetwork()
        {
            ExecutionCompleted = false;

            isNetworkCreator = false;
            NetworkId = string.Empty;
            Manager.LeaveNetwork();

            yield return WaitForExecution();
        }

        /// <summary>
        /// Adds a listener for the OnChatMessageReceived event.
        /// </summary>
        /// <param name="listener">Delegete <see cref="Action"/> with Sender (<see cref="object"/>), Player (<see cref="PlayFabPlayer"/>), message (<see cref="string"/>), and type (<see cref="ChatMessageType"/>.</param>
        public void AddOnChatMessageReceivedListener(Action<object, PlayFabPlayer, string, ChatMessageType> listener)
        {
            if (listener != null)
            {
                Manager.OnChatMessageReceived += (sender, from, message, type) => { listener(sender, from, message, type); };
            }
        }

        /// <summary>
        /// Adds a listener for the OnDataMessageNoCopyReceived event.
        /// </summary>
        /// <param name="listener">Delegete <see cref="Action"/> with Sender (<see cref="object"/>), Player (<see cref="PlayFabPlayer"/>), buffer (<see cref="IntPtr"/>), and buffer size (<see cref="uint"/>).</param>
        public void AddOnDataMessageNoCopyReceivedListener(Action<object, PlayFabPlayer, IntPtr, uint> listener)
        {
            if (listener != null)
            {
                Manager.OnDataMessageNoCopyReceived += (sender, from, buffer, bufferSize) => { listener(sender, from, buffer, bufferSize); };
            }
        }

        /// <summary>
        /// Adds a listener for the OnDataMessageReceived event.
        /// </summary>
        /// <param name="listener">Delegete <see cref="Action"/> with Sender (<see cref="object"/>), Player (<see cref="PlayFabPlayer"/>), and buffer (<see cref="byte"/>).</param>
        public void AddOnDataMessageReceivedListener(Action<object, PlayFabPlayer, byte[]> listener)
        {
            if (listener != null)
            {
                Manager.OnDataMessageReceived += (sender, from, buffer) => { listener(sender, from, buffer); };
            }
        }

        /// <summary>
        /// Adds a listener for the OnError event.
        /// </summary>
        /// <param name="listener">Delegate <see cref="Action"/> with a Sender (<see cref="object"/>) and args (<see cref="PlayFabMultiplayerManagerErrorArgs"/>) parameters.</param>
        public void AddOnErrorListener(Action<object, PlayFabMultiplayerManagerErrorArgs> listener)
        {
            if (listener != null)
            {
                Manager.OnError += (sender, args) => { listener(sender, args); };
            }
        }

        /// <summary>
        /// Adds a listener for the OnNetworkChanged event.
        /// </summary>
        /// <param name="listener">Delegate <see cref="Action"/> with a Sender (<see cref="object"/>) and NetworkId (<see cref="string"/>) parameters.</param>
        public void AddOnNetworkChangeListener(Action<object, string> listener)
        {
            if (listener != null)
            {
                Manager.OnNetworkChanged += (sender, networkId) => { listener(sender, networkId); };
            }
        }

        /// <summary>
        /// Adds a listener for the OnNetworkJoined event.
        /// </summary>
        /// <param name="listener">Delegate <see cref="Action"/> with a Sender (<see cref="object"/>) and NetworkId (<see cref="string"/>) parameters.</param>
        public void AddOnNetworkJoinedListener(Action<object, string> listener)
        {
            if (listener != null)
            {
                Manager.OnNetworkJoined += (sender, networkId) => { listener(sender, networkId); };
            }
        }

        /// <summary>
        /// Adds a listener for the OnNetworkLeft event.
        /// </summary>
        /// <param name="listener">Delegate <see cref="Action"/> with a Sender (<see cref="object"/>) and NetworkId (<see cref="string"/>) parameters.</param>
        public void AddOnNetworkLeftListener(Action<object, string> listener)
        {
            if (listener != null)
            {
                Manager.OnNetworkLeft += (sender, networkId) => { listener(sender, networkId); };
            }
        }

        /// <summary>
        /// Adds a listener for the OnRemotePlayerJoined event.
        /// </summary>
        /// <param name="listener">Delegate <see cref="Action"/> with Sender (<see cref="object"/>) and Player (<see cref="PlayFabPlayer"/>) parameters.</param>
        public void AddOnRemotePlayerJoinedListener(Action<object, PlayFabPlayer> listener)
        {
            if (listener != null)
            {
                Manager.OnRemotePlayerJoined += (sender, player) => { listener(sender, player); };
            }
        }

        /// <summary>
        /// Adds a listener for the OnRemotePlayerLeft event.
        /// </summary>
        /// <param name="listener">Delegate <see cref="Action"/> with Sender (<see cref="object"/>) and Player (<see cref="PlayFabPlayer"/>) parameters.</param>
        public void AddOnRemotePlayerLeftListener(Action<object, PlayFabPlayer> listener)
        {
            if (listener != null)
            {
                Manager.OnRemotePlayerLeft += (sender, player) => { listener(sender, player); };
            }
        }

        /// <summary>
        /// Sends a message to the Players connected to the current Network. NOTE: the message won't be received by the Local player.
        /// </summary>
        /// <param name="message">A <see cref="PartyNetworkMessageWrapper"/> object.</param>
        public void SendDataMessage<T>(PartyNetworkMessageWrapper<T> message)
        {
            PartyNetworkMessageHelper.BufferData(message, out var buffer, out _);

            Manager.SendDataMessage(buffer, Manager.RemotePlayers, DeliveryOption.BestEffort);
        }

        private void OnNetworkJoined(object sender, string networkId)
        {
            NetworkId = networkId;
            ExecutionCompleted = true;
        }

        private void OnNetworkLeft(object sender, string networkId)
        {
            NetworkId = null;
            ExecutionCompleted = true;
        }
    }
}

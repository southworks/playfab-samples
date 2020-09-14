// Copyright (C) Microsoft Corporation. All rights reserved.

using PlayFab;
using PlayFab.MultiplayerModels;
using System.Collections;
using TicTacToe.Models;
using TicTacToe.Models.Helpers;
using UnityEngine;

namespace TicTacToe.Handlers
{
    public class MatchmakingHandler : RequestHandler
    {
        private MatchmakingQueueConfiguration QueueConfiguration { get; set; }

        public CreateMatchmakingTicketResult MatchmakingTicketResult { get; set; }

        public GetMatchmakingTicketResult MatchmakingTicketStatus { get; set; }

        public GetMatchResult MatchResult { get; set; }

        public MatchmakingHandler(PlayerInfo player, MatchmakingQueueConfiguration queueConfiguration) : base(player)
        {
            QueueConfiguration = queueConfiguration;
        }

        public IEnumerator CreateMatchmakingTicket(string attributeValue)
        {
            ExecutionCompleted = false;

            PlayFabMultiplayerAPI.CreateMatchmakingTicket(
                new CreateMatchmakingTicketRequest
                {
                    Creator = new MatchmakingPlayer
                    {
                        Attributes = GetMatchmakingAttribute(attributeValue),
                        Entity = new EntityKey
                        {
                            Id = Player.Entity.Id,
                            Type = Player.Entity.Type,
                        },
                    },
                    GiveUpAfterSeconds = QueueConfiguration.GiveUpAfterSeconds,
                    QueueName = QueueConfiguration.QueueName,
                    AuthenticationContext = PlayFabAuthenticationContext
                },
                (result) =>
                {
                    ExecutionCompleted = true;
                    MatchmakingTicketResult = result;
                },
                (error) =>
                {
                    var result = $"On CreateMatchmakingTicket failed. Message: {error.ErrorMessage}, Code: {error.HttpCode}";
                    Debug.Log(result);
                }
            );

            yield return WaitForExecution();
        }

        public IEnumerator GetMatchmakingTicketStatus(bool escapeObject = false)
        {
            ExecutionCompleted = false;

            PlayFabMultiplayerAPI.GetMatchmakingTicket(
                new GetMatchmakingTicketRequest
                {
                    EscapeObject = escapeObject,
                    QueueName = QueueConfiguration.QueueName,
                    TicketId = MatchmakingTicketResult.TicketId,
                    AuthenticationContext = PlayFabAuthenticationContext
                },
                (result) =>
                {
                    ExecutionCompleted = true;
                    MatchmakingTicketStatus = result;
                },
                (error) =>
                {
                    var result = $"On GetMatchmakingTicketStatus failed. Message: {error.ErrorMessage}, Code: {error.HttpCode}";
                    Debug.Log(result);
                }
            );

            yield return WaitForExecution();
        }

        public IEnumerator CancelAllMatchmakingTicketsForPlayer()
        {
            ExecutionCompleted = false;

            PlayFabMultiplayerAPI.CancelAllMatchmakingTicketsForPlayer(
                new CancelAllMatchmakingTicketsForPlayerRequest
                {
                    QueueName = QueueConfiguration.QueueName,
                    Entity = new EntityKey
                    {
                        Id = Player.Entity.Id,
                        Type = Player.Entity.Type,
                    },
                    AuthenticationContext = PlayFabAuthenticationContext
                },
                (result) =>
                {
                    ExecutionCompleted = true;
                    // set both objects to null as the related ticket was cancelled
                    MatchmakingTicketResult = null;
                    MatchmakingTicketStatus = null;
                },
                (error) =>
                {
                    var result = $"On CancelAllMatchmakingTicketsForPlayer failed. Message: {error.ErrorMessage}, Code: {error.HttpCode}";
                    Debug.Log(result);
                }
            );

            yield return WaitForExecution();
        }

        public IEnumerator EnsureGetMatchmakingTicketStatus()
        {
            yield return GetMatchmakingTicketStatus();

            while (MatchmakingTicketStatus != null && MatchmakingTicketStatus.Status != MatchmakingTicketStatusEnum.Matched && MatchmakingTicketStatus.Status != MatchmakingTicketStatusEnum.Canceled)
            {
                yield return GetMatchmakingTicketStatus();
                Debug.Log($"Matchmaking Ticket Status: { MatchmakingTicketStatus.Status }");

                if (MatchmakingTicketStatus.Status == MatchmakingTicketStatusEnum.Canceled)
                {
                    MatchmakingTicketStatus = null;
                }

                if (MatchmakingTicketStatus.Status != MatchmakingTicketStatusEnum.Matched)
                {
                    yield return new WaitForSeconds(Constants.RETRY_GET_TICKET_STATUS_AFTER_SECONDS);
                }
            };

            yield return WaitForExecution();
        }

        public IEnumerator GetMatchInfo(bool escapeObject = false, bool returnMemberAttributes = false)
        {
            ExecutionCompleted = false;

            PlayFabMultiplayerAPI.GetMatch(
                new GetMatchRequest
                {
                    EscapeObject = escapeObject,
                    MatchId = MatchmakingTicketStatus.MatchId,
                    QueueName = QueueConfiguration.QueueName,
                    ReturnMemberAttributes = returnMemberAttributes,
                    AuthenticationContext = PlayFabAuthenticationContext,
                },
                (result) =>
                {
                    ExecutionCompleted = true;
                    MatchResult = result;
                },
                (error) =>
                {
                    var result = $"On GetMatchInfo failed. Message: {error.ErrorMessage}, Code: {error.HttpCode}";
                    Debug.Log(result);
                }
            );

            yield return WaitForExecution();
        }

        private MatchmakingPlayerAttributes GetMatchmakingAttribute(string attributeValue)
        {
            var attributes = new MatchmakingPlayerAttributes();

            attributes.DataObject = QueueConfiguration.IsQuickPlay ?
                 (new { Skill = attributeValue }) :
                (object)new { LobbyId = attributeValue };

            return attributes;
        }

    }
}
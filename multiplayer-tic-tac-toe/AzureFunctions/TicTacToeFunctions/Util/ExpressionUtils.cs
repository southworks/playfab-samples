// Copyright (C) Microsoft Corporation. All rights reserved.

using Microsoft.Azure.Documents;
using System;
using System.Linq.Expressions;
using TicTacToeFunctions.Models;

namespace TicTacToeFunctions.Util
{
    public class ExpressionUtils
    {
        public static Expression<Func<MatchLobby, bool>> GetSearchMatchLobbiesExpression(string filter)
        {
            Func<MatchLobby, bool> result = null;
            Func<MatchLobby, bool> matchIdFunc = (mLobbyInfo) => true;
            Func<MatchLobby, bool> nameFunc = (mLobbyInfo) => true;

            if (!string.IsNullOrWhiteSpace(filter))
            {
                matchIdFunc = (item) => item.MatchLobbyId == filter;
            }

            result = (mLobbyInfo) => matchIdFunc(mLobbyInfo) || nameFunc(mLobbyInfo);

            // return func as an expression
            return (x) => result(x);
        }

        public static Expression<Func<Document, bool>> GetDeleteDocumentByIdExpression(string Id)
        {
            if (Id == null)
            {
                Id = string.Empty;
            }

            Func<Document, bool> result = (document) => document.Id == Id;

            return (x) => result(x);
        }
    }
}

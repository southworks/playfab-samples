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
            Func<MatchLobby, bool> matchIdFunc = (mLobbyInfo) => true;

            if (!string.IsNullOrWhiteSpace(filter))
            {
                matchIdFunc = (item) => item.MatchLobbyId.IndexOf(filter.Trim(), StringComparison.OrdinalIgnoreCase) != -1;
            }

            return (mLobbyInfo) => matchIdFunc(mLobbyInfo);
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

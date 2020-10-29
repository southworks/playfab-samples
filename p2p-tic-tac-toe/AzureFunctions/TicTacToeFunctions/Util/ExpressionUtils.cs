using System;
using System.Linq.Expressions;
using Microsoft.Azure.Documents;
using TicTacToeFunctions.Models.Responses;

namespace TicTacToeFunctions.Util
{
    public class ExpressionUtils
    {
        public static Expression<Func<MatchLobbyDTO, bool>> GetSearchMatchLobbiesExpression(string filter)
        {
            Func<MatchLobbyDTO, bool> matchIdFunc = (mLobbyInfo) => true;

            if (!string.IsNullOrWhiteSpace(filter))
            {
                matchIdFunc = (item) => item.MatchLobbyId.IndexOf(filter.Trim(), StringComparison.OrdinalIgnoreCase) != -1;
            }

            return (mLobbyInfo) => matchIdFunc(mLobbyInfo);
        }

        public static Expression<Func<Document, bool>> GetDocumentByIdExpression(string id)
        {
            if (id == null)
            {
                id = string.Empty;
            }

            Func<Document, bool> result = (document) => document.Id == id;

            return (x) => result(x);
        }
    }
}

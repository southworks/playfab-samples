using PlayFab;
using System;
using System.Threading.Tasks;
using TicTacToeFunctions.Models;

namespace TicTacToeFunctions.Util
{
    public static class MatchUtil
    {
        public static async Task<TicTacToeSharedGroupData> AddMember(PlayFabAuthenticationContext context, string sharedGroupId, string playerId)
        {
            var sharedGroup = await SharedGroupDataUtil.GetAsync(context, sharedGroupId);

            if (!string.IsNullOrWhiteSpace(sharedGroup.Match.PlayerOneId) && !string.IsNullOrWhiteSpace(sharedGroup.Match.PlayerTwoId))
            {
                throw new Exception("Match is full");
            }

            if (string.Compare(sharedGroup.Match.PlayerOneId, playerId, true) == 0)
            {
                throw new Exception("This player is already the player one");
            }

            sharedGroup.Match.PlayerTwoId = playerId;

            return await SharedGroupDataUtil.UpdateAsync(context, sharedGroup);
        }
    }
}

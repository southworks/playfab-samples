using System.Collections;
using TicTacToe.Models.Helpers;

namespace TicTacToe.Models
{
    public interface IMatchmakingHandler
    {
        IEnumerator CreateTicket(string attribute, string networkId = "");

        IEnumerator GetTicketStatus();

        IEnumerator CancelPlayerTickets();

        IEnumerator GetMatch();

        IEnumerator EnsureGetTicketStatus();

        MatchmakingQueueConfiguration QueueConfiguration { get; set; }
    }
}

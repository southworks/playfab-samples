using TicTacToe.Models.Helpers;

namespace TicTacToe.Models
{
    public interface IMatchmakingQueueConfiguration
    {
        void ChangeQueueConfiguration(QueueTypes queueType);
    }
}

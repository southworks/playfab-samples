using System.Collections;
using UnityEngine;

namespace TicTacToe.Handlers
{
    public class RequestHandler
    {
        protected bool ExecutionCompleted { get; set; }

        protected IEnumerator WaitForExecution()
        {
            yield return new WaitUntil(() => { return ExecutionCompleted; });
        }

    }
}

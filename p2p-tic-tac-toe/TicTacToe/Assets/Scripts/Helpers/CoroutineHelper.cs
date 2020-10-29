using System;
using System.Collections;

namespace TicTacToe.Helpers
{
    public class CoroutineHelper
    {
        public static IEnumerator Run<T>(IEnumerator coroutineReturn, Action<T> outputHandler)
        {
            object output = null;
            while (coroutineReturn.MoveNext())
            {
                output = coroutineReturn.Current;
                yield return output;
            }
            outputHandler((T)output);
        }
    }
}

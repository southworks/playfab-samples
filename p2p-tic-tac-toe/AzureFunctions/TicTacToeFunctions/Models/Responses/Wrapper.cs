using System.Collections.Generic;

namespace TicTacToeFunctions.Models.Responses
{
    public class Wrapper<T>
    {
        public List<T> Items { get; set; }
    }
}

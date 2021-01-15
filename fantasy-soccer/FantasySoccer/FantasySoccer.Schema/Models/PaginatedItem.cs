using System.Collections.Generic;

namespace FantasySoccer.Schema.Models
{
    public class PaginatedItem<T>
    {
        public List<T> PaginatedItems { get; set; }

        public int TotalItems { get; set; }
    }
}

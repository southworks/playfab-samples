using System;

namespace FantasySoccer.Core.Services.Exceptions
{
    public class TournamentStartDateMustBeOlderThanEndDateException: Exception
    {
        public TournamentStartDateMustBeOlderThanEndDateException(string message) : base(message) { }
    }
}

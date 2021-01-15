namespace FantasySoccer.Schema.Models.PlayFab
{
    /// <summary>
    /// We shouldn't use this class until we are sure
    /// we are going to do something with this data
    /// </summary>
    public class FutbolPlayerTournamentStats : GeneralModel
    {
        public string FutbolPlayerID { get; set; }
        public string TournamentID { get; set; }
        public int Goals { get; set; }
        public int Assists { get; set; }
        public int Wins { get; set; }
        public int Draws { get; set; }
        public int Losses { get; set; }
    }
}

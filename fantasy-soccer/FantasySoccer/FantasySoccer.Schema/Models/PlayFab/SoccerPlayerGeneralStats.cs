namespace FantasySoccer.Schema.Models.PlayFab
{
    public class SoccerPlayerGeneralStats : GeneralModel
    {
        public string FutbolPlayerID { get; set; }
        public int Goals { get; set; }
        public int Assists { get; set; }
        public int Wins { get; set; }
        public int Draws { get; set; }
        public int Losses { get; set; }
    }
}
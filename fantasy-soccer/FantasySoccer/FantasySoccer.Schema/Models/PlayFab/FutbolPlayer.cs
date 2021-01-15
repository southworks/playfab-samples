using System;

namespace FantasySoccer.Schema.Models.PlayFab
{
    public class FutbolPlayer: GeneralModel
    {
        public string InventoryId { get; set; }
        public string FutbolTeamID { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public DateTime Birthdate { get; set; }
        public uint Price { get; set; }
        public Position Position { get; set; }
        public bool IsStarter { get; set; }
        public SoccerPlayerGeneralStats GeneralStats { get; set; }
        public int ProbabilityOfGoal { get; set; }

        public string GetFullName()
        {
            return $"{LastName} {Name}";
        }
    }
}

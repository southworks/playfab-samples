using System;
using System.ComponentModel.DataAnnotations;
using FantasySoccer.Models.Responses;

namespace FantasySoccer.Models.ViewModels
{
    public class TournamentManagementViewModel
    {
        public string Id { get; set; }
        
        [Required(ErrorMessage = "Enter a tournament name")]
        [Display(Name = "Tournament Name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Enter the start date of the tournament")]
        [DisplayFormat(DataFormatString = "{0:dd MMM yyyy}")]
        [DataType(DataType.Date)]
        [Display(Name = "Start date tournament")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Enter the end date of the tournament")]
        [DisplayFormat(DataFormatString = "{0:dd MMM yyyy}")]
        [DataType(DataType.Date)]
        [Display(Name = "End date tournament")]
        public DateTime EndDate { get; set; }
        
        public BaseResponseWrapper Result { get; set; }
    }
}

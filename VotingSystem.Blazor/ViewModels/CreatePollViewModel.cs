using System.ComponentModel.DataAnnotations;

namespace VotingSystem.Blazor.ViewModels
{
    public class CreatePollViewModel
    {
        [Required]
        [MinLength(5, ErrorMessage = "The question must be at least 5 characters")]
        public string Question { get; set; } = string.Empty;

        [Required(ErrorMessage = "Start date is required")]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "End date is required")]
        public DateTime EndDate { get; set; } = DateTime.Today.AddDays(1);

        public List<PollOptionViewModel> Options { get; set; } = new()
    {
        new PollOptionViewModel(),
        new PollOptionViewModel()
    };
    }
}

using System.ComponentModel.DataAnnotations;

namespace VotingSystem.Blazor.ViewModels
{
    public class PollOptionViewModel
    {
        public Guid Id { get; set; } = new Guid();

        public string Text { get; set; } = string.Empty;
    }
}

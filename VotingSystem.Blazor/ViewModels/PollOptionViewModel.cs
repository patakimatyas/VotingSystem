using System.ComponentModel.DataAnnotations;

namespace VotingSystem.Blazor.ViewModels
{
    public class PollOptionViewModel
    {
        public int? Id { get; set; }
        
        public Guid TempKey { get; set; } = new Guid();

        public string Text { get; set; } = string.Empty;


    }
}

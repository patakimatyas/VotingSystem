namespace VotingSystem.Blazor.ViewModels
{
    public class PollDetailsViewModel
    {
        public int Id { get; set; }
        public string Question { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<OptionViewModel> Options { get; set; } = new();
        public List<VoterStatusViewModel> Voters { get; set; } = new();

    }

    public class OptionViewModel
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public int VoteCount { get; set; }
    }

    public class VoterStatusViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool HasVoted { get; set; }
    }
}

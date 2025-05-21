using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VotingSystem.Shared.DTOs
{
    public class PollResponseDTO
    {
        public int Id { get; set; }
        public string Question { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<OptionResponseDTO> Options { get; set; } = new();
        public List<VoterStatusDTO> Voters { get; set; } = new();
        public bool HasVoted { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VotingSystem.Shared.DTOs
{
    public class VoteRequestDTO
    {
        public int OptionId { get; set; }
        public int PollId { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VotingSystem.DataAccess.Models
{
    public class Voter
    {
        public string UserId { get; set; } = null!;
        public virtual ApplicationUser User { get; set; } = null!;

        public int PollId { get; set; }
        public virtual Poll Poll { get; set; } = null!;
    }

}

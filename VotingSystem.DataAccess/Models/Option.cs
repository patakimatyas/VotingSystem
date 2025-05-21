using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VotingSystem.DataAccess.Models
{
    public class Option
    {
        public int Id { get; set; }
        public string Text  { get; set; } = string.Empty;
        public int VoteCount { get; set; }

        [ForeignKey("Poll")]
        public int PollId { get; set; }

        public virtual Poll Poll { get; set; } = null!;
    }
}

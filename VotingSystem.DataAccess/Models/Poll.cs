using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;


namespace VotingSystem.DataAccess.Models
{
    public class Poll
    {
        [Key]
        public int Id { get; set; }

        public string Question { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsClosed { get; set; } = false;

        public string? CreatedByUserId { get; set; }
        public virtual ApplicationUser? CreatedByUser { get; set; }

        public virtual ICollection<Voter> Voters { get; set; } = new List<Voter>();
        public virtual ICollection<Option> Options { get; set; } = new List<Option>();
        public virtual ICollection<Vote> Votes { get; set; } = new List<Vote>();
    }
}

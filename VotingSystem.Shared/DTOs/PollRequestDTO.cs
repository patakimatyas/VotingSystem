using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VotingSystem.Shared.DTOs
{
    public class PollRequestDTO
    {
    
        // Ezt használja az admin felület vagy az API a szavazás létrehozására

        public string Question { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<string> Options { get; set; } = new();
        
    }
}

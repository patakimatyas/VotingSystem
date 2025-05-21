using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VotingSystem.Shared.DTOs
{
    public class OptionResponseDTO
    {
        //Egyetlen opció adatait és a rá érkezett szavazatokat tartalmazza

        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public int VoteCount { get; set; }
        public double VotePercentage { get; set; }
    }
}

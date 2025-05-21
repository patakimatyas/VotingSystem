using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VotingSystem.Shared.DTOs
{
    public class UserResponseDTO
    {
        
        public required string Id { get; init; }

        public required string Name { get; init; }

        public required string Email { get; init; }
    }
}

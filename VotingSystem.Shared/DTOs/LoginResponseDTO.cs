using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VotingSystem.Shared.DTOs
{
    public class LoginResponseDTO
    {
        public required string AuthToken { get; init; }
        public required string RefreshToken { get; init; }
        public required string UserId { get; init; }
    }
}

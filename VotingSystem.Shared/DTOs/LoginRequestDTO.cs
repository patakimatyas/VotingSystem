using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VotingSystem.Shared.DTOs
{
    public class LoginRequestDTO
    {
        
        [EmailAddress(ErrorMessage = "Email is invalid")]
        public required string Email { get; init; }

        public required string Password { get; init; }
    }
}

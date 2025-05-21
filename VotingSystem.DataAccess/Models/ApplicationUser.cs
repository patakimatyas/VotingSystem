using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VotingSystem.DataAccess.Models
{
    public class ApplicationUser : IdentityUser
    {
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;
        public Guid? RefreshToken { get; set; }
    }
}

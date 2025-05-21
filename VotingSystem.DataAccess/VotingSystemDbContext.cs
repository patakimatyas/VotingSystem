using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VotingSystem.DataAccess.Models;

namespace VotingSystem.DataAccess
{
    public class VotingSystemDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Poll> Polls => Set<Poll>();
        public DbSet<Option> Options => Set<Option>();
        public DbSet<Vote> Votes => Set<Vote>();
        public DbSet<Voter> Voters => Set<Voter>();

        public VotingSystemDbContext(DbContextOptions<VotingSystemDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Poll <-> CreatedByUser
            builder.Entity<Poll>()
                .Property(p => p.Id)
                .ValueGeneratedOnAdd();

            builder.Entity<Poll>()
                .HasOne(p => p.CreatedByUser)
                .WithMany()
                .HasForeignKey(p => p.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Option setup
            builder.Entity<Option>()
                .Property(o => o.Id)
                .ValueGeneratedOnAdd();

            // Vote setup
            builder.Entity<Vote>()
                .HasOne(v => v.Poll)
                .WithMany(p => p.Votes)
                .HasForeignKey(v => v.PollId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Vote>()
                .HasOne(v => v.Option)
                .WithMany()
                .HasForeignKey(v => v.OptionId)
                .OnDelete(DeleteBehavior.Restrict);

            // ----------- VOTER (JOIN ENTITY) SETUP -----------
            builder.Entity<Voter>()
                .HasKey(v => new { v.UserId, v.PollId });

            builder.Entity<Voter>()
                .HasOne(v => v.User)
                .WithMany()
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Voter>()
                .HasOne(v => v.Poll)
                .WithMany(p => p.Voters)
                .HasForeignKey(v => v.PollId)
                .OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(builder);
        }
    }
}

using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VotingSystem.DataAccess;
using VotingSystem.DataAccess.Models;

namespace VotingSystem.DataAccess
{
    public static class DbInitializer
    {
        public static void Initialize(VotingSystemDbContext context, UserManager<ApplicationUser> userManager)
        {
            if (context.Polls.Any()) return;

            var userEmail = "teszt@gmail.com";
            var user = userManager.FindByEmailAsync(userEmail).Result;
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = userEmail,
                    Email = userEmail,
                    EmailConfirmed = true
                };
                var result = userManager.CreateAsync(user, "Teszt123!").Result;
            }
            var userEmail2 = "teszt2@gmail.com";
            var user2 = userManager.FindByEmailAsync(userEmail2).Result;
            if (user2 == null)
            {
                user2 = new ApplicationUser
                {
                    UserName = userEmail,
                    Email = userEmail,
                    EmailConfirmed = true
                };
                var result = userManager.CreateAsync(user2, "Teszt123!").Result;
            }

            var poll1 = new Poll
            {
                Question = "Melyik a kedvenc programozási nyelved?",
                IsClosed = false,
                StartDate = DateTime.Now.AddDays(-1),
                EndDate = DateTime.Now.AddDays(5),
                CreatedByUserId = user.Id,
                Options = new List<Option>
                {
                    new Option { Text = "C#" },
                    new Option { Text = "Python" },
                    new Option { Text = "JavaScript" }
                }
               
            };

            var poll2 = new Poll
            {
                Question = "Szerinted érdemes Blazort tanulni?",
                IsClosed = false,
                StartDate = DateTime.Now.AddDays(-2),
                EndDate = DateTime.Now.AddDays(3),
                Options = new List<Option>
                {
                    new Option { Text = "Igen" },
                    new Option { Text = "Nem" }
                }
            };

            var poll3 = new Poll
            {
                Question = "Tanulnál csak .Net-et és Szoftech-et a sok matek helyett?",
                IsClosed = false,
                StartDate = DateTime.Now.AddDays(-3),
                EndDate = DateTime.Now.AddDays(7),
                Options = new List<Option>
                {
                    new Option { Text = "Nyilván" },
                    new Option { Text = "Természetesen" }
                }
            };

            var poll4 = new Poll
            {
                Question = "Lejárt teszt kérdés?",
                IsClosed = false,
                StartDate = DateTime.Now.AddDays(-3),
                EndDate = DateTime.Now.AddDays(-2),
                Options = new List<Option>
                {
                    new Option { Text = "A" },
                    new Option { Text = "B" }
                }
            };

            var poll5 = new Poll
            {
                Question = "Második lejárt teszt kérdés?",
                IsClosed = false,
                StartDate = DateTime.Now.AddDays(-4),
                EndDate = DateTime.Now.AddDays(-3),
                Options = new List<Option>
                {
                    new Option { Text = "A" },
                    new Option { Text = "B" },
                    new Option { Text = "C" }
                }
            };
            context.AddRange(poll1, poll2, poll3, poll4, poll5);
            context.SaveChanges();


        }
    }
}

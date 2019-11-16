using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using contractWhist2.Models;
using Microsoft.EntityFrameworkCore;

namespace contractWhist2.Data
{
    public class whistContext : DbContext
    {
        public whistContext(DbContextOptions<whistContext> options)
            : base(options)
        { 
        }

        public DbSet<card> card { get; set; }
        public DbSet<gameRound> gameRound { get; set; }
        public DbSet<playerRoundCard> playerRoundCard { get; set; }
        public DbSet<game> game { get; set; }
        public DbSet<playerRound> playerRound { get; set; }



    }
}

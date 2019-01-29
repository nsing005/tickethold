using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TicketHold.Core.Entities;

namespace TicketHold.Core
{
    public class TicketContext : DbContext
    {
        public TicketContext(DbContextOptions options) : base(options)
        {

        }

        public DbSet<SeatHold> SeatHolds { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SeatHold>().HasKey(t => t.SeatHoldId);
        }

    }
}

﻿using BTM.Account.Domain.Users;
using BTM.Account.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BTM.Account.Infrastructure
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) // This passes the options to the base constructor of DbContext
        {
        }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                        .HasKey(u => u.Id);
        }
    }
}

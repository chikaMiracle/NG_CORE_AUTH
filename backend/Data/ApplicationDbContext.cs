﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApiAngular.Models;

namespace WebApiAngular.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {


        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): base(options)
        {

        }

        //Creating Role for our application
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<IdentityRole>().HasData(
                      new { Id = "1", Name = "Admin", NormalizedName = "ADMIN" },
                      new { Id = "2", Name = "Customer", NormalizedName = "CUSTOMER" },
                      new { Id = "3", Name = "Moderator", NormalizedName = "MODERATOR" }

                );
        }


        public DbSet<ProductModel> Product { get; set; }
    }

   
}

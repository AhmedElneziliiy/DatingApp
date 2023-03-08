using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class DataContext : DbContext
    {   
        public DataContext(DbContextOptions options) : base(options)
        {
            
        }


        public DbSet<AppUser>Users { get; set; }
        public DbSet<UserLike> Likes { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<UserLike>()
                .HasKey(k=>new{k.LikedUserId,k.SourceUserId});
                
            builder.Entity<UserLike>()
                .HasOne(s=>s.SourceUser)
                .WithMany(d=>d.LikedUsers)
                .HasForeignKey(f=>f.SourceUserId)
                .OnDelete(DeleteBehavior.Cascade);
           
            builder.Entity<UserLike>()
                .HasOne(s=>s.LikedUser)
                .WithMany(d=>d.LikedByUser)
                .HasForeignKey(f=>f.LikedUserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
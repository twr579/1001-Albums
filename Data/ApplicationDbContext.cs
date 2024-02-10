using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using _1001Albums.Models;
using System.Reflection.Metadata;

namespace _1001Albums.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<_1001Albums.Models.Album> Album { get; set; } = default!;
        public DbSet<_1001Albums.Models.UserRating> UserRating { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Album>()
                .HasMany(e => e.UserRatings)
                .WithOne(e => e.Album)
                .HasForeignKey(e => e.AlbumId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<UserRating>().HasKey(r => new { r.AlbumId, r.UserId });

        }
    }
}

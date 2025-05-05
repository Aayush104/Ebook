using BookProject.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using System.Reflection.Emit;
using static System.Net.WebRequestMethods;

namespace BookProject.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {


        }

        public DbSet<ApplicationUser> Users { get; set; }
        public DbSet<Otp> Otps { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Bookmark> BookMarks { get; set; }
        public DbSet<Cart> Carts { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Otp>()
               .HasOne(o => o.User)
               .WithMany(u => u.Otps)
               .HasForeignKey(o => o.UserId)
               .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Bookmark>()
             .HasOne(o => o.User)
             .WithMany(u => u.Bookmarks)
             .HasForeignKey(o => o.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Bookmark>()
            .HasOne(o => o.Book)
            .WithMany(u => u.Bookmarks)
            .HasForeignKey(o => o.BookId)
            .OnDelete(DeleteBehavior.Cascade);


            builder.Entity<Cart>()
             .HasOne(o => o.User)
             .WithMany(u => u.Carts)
             .HasForeignKey(o => o.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Cart>()
          .HasOne(o => o.Book)
          .WithMany(u => u.Carts)
          .HasForeignKey(o => o.BookId)
          .OnDelete(DeleteBehavior.Cascade);

            builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}

using Microsoft.EntityFrameworkCore;
using SmartSocietyMVC.Models;

namespace SmartSocietyMVC.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Society> Societies { get; set; }
        public DbSet<Facility> Facilities { get; set; }
        public DbSet<Notice> Notices { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Complaint> Complaints { get; set; }
        public DbSet<Bill> Bills { get; set; }
        public DbSet<Event> Events { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // User to Society: A User belongs to a Society (Optional depending on your logic)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Society)
                .WithMany(s => s.Users)
                .HasForeignKey(u => u.SocietyId)
                .OnDelete(DeleteBehavior.Restrict);

            // Facility to Society
            modelBuilder.Entity<Facility>()
                .HasOne(f => f.Society)
                .WithMany(s => s.Facilities)
                .HasForeignKey(f => f.SocietyId)
                .OnDelete(DeleteBehavior.Cascade);

            // Notice to Society
            modelBuilder.Entity<Notice>()
                .HasOne(n => n.Society)
                .WithMany(s => s.Notices)
                .HasForeignKey(n => n.SocietyId)
                .OnDelete(DeleteBehavior.Cascade);

            // Event to Society
            modelBuilder.Entity<Event>()
                .HasOne(e => e.Society)
                .WithMany()
                .HasForeignKey(e => e.SocietyId)
                .OnDelete(DeleteBehavior.Cascade);

            // User to Bookings
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Facility to Bookings
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Facility)
                .WithMany(f => f.Bookings)
                .HasForeignKey(b => b.FacilityId)
                .OnDelete(DeleteBehavior.Cascade);

            // User to Complaints
            modelBuilder.Entity<Complaint>()
                .HasOne(c => c.User)
                .WithMany(u => u.Complaints)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Society to Complaints (Optional, but good for admin dashboard scoping)
            modelBuilder.Entity<Complaint>()
                .HasOne(c => c.Society)
                .WithMany(s => s.Complaints)
                .HasForeignKey(c => c.SocietyId)
                .OnDelete(DeleteBehavior.Restrict);


            // User to Bills
            modelBuilder.Entity<Bill>()
                .HasOne(b => b.User)
                .WithMany(u => u.Bills)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Горнолыжный_комплекс__Благодать_.Models;

namespace Горнолыжный_комплекс__Благодать_
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Client> Clients { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<LoginHistory> LoginHistories { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderService> OrderServices { get; set; }

        public ApplicationDbContext() : base() { }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(
                    "Server=DESKTOP-KEHORP4;Database=BlagodatRental;Trusted_Connection=True;TrustServerCertificate=True",
                    options => options.EnableRetryOnFailure()); 
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LoginHistory>().ToTable("LoginHistory");
            modelBuilder.Entity<OrderService>()
                .HasKey(os => new { os.OrderID, os.ServiceID });

            modelBuilder.Entity<OrderService>()
                .HasOne(os => os.Order)
                .WithMany(o => o.OrderServices)
                .HasForeignKey(os => os.OrderID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderService>()
                .HasOne(os => os.Service)
                .WithMany()
                .HasForeignKey(os => os.ServiceID)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
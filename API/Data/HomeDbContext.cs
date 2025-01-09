using API.Models.Devices;
using API.Models.Shared;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class HomeDbContext : DbContext
{
    public HomeDbContext(DbContextOptions<HomeDbContext> options) : base(options)
    {
    }
    
    public DbSet<Log> Logs { get; set; }
    
    public DbSet<Device> Devices { get; set; }
    
    // ip address and mac address must be unique
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Device>()
            .HasIndex(d => d.IP)
            .IsUnique();
        
        modelBuilder.Entity<Device>()
            .HasIndex(d => d.MacAddress)
            .IsUnique();
    }
}
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
}
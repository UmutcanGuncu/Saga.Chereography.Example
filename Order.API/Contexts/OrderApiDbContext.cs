using Microsoft.EntityFrameworkCore;
using Order.API.Models;

namespace Order.API.Contexts;

public class OrderApiDbContext: DbContext
{
    public OrderApiDbContext(DbContextOptions<OrderApiDbContext> options) : base(options)
    {
        
    }
    public DbSet<Models.Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
}
using Microsoft.EntityFrameworkCore;
using OnlineStoreRestfulApi.Models;

namespace OnlineStoreRestfulApi.Datas;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
}

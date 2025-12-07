using System.Reflection;
using LuShop.Api.Models; 
using LuShop.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LuShop.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<
        User, 
        IdentityRole<long>, 
        long,
        IdentityUserClaim<long>,
        IdentityUserRole<long>,
        IdentityUserLogin<long>,
        IdentityRoleClaim<long>,
        IdentityUserToken<long>
    >(options)
{
    // Usamos '= null!;' para silenciar o aviso de nulo, 
    // pois o EF Core injeta esses valores em tempo de execução.
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<OrderItem> OrderItems { get; set; } = null!;
    public DbSet<Cart> Carts { get; set; } = null!;
    public DbSet<CartItem> CartItems { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<Voucher> Vouchers { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 1. IMPORTANTE: Configura as tabelas do Identity (AspNetUsers, AspNetRoles, etc.)
        // Isso é essencial e deve vir antes dos seus mapeamentos.
        base.OnModelCreating(modelBuilder); 

        // 2. Aplica os mapeamentos que criamos (OrderMapping, ProductMapping, etc.)
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // O bloco ConfigurePasskeys foi removido para resolver os erros de "Cannot resolve symbol",
        // pois esses tipos não são mais facilmente acessíveis ou foram movidos no .NET 9.
        // O suporte a Passkeys/WebAuthn agora é configurado principalmente no Program.cs.
    }
}
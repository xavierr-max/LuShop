using LuShop.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LuShop.Api.Data.Mappings;

public class OrderMapping : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        // 1. Tabela
        builder.ToTable("Orders");

        // 2. Chave Primária
        builder.HasKey(x => x.Id);

        // 3. Propriedades
        builder.Property(x => x.Number)
            .IsRequired()
            .HasColumnType("CHAR") // Otimização: Tamanho fixo é mais rápido
            .HasMaxLength(8);

        builder.Property(x => x.ExternalReference)
            .IsRequired(false) // Pode ser nulo
            .HasColumnType("VARCHAR")
            .HasMaxLength(60); // Tamanho suficiente para IDs do Stripe/PayPal

        // Enums geralmente são mapeados como SMALLINT ou INT no banco
        builder.Property(x => x.Gateway)
            .IsRequired()
            .HasColumnType("SMALLINT");

        builder.Property(x => x.Status)
            .IsRequired()
            .HasColumnType("SMALLINT");

        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasColumnType("DATETIME")
            .HasDefaultValueSql("GETDATE()"); // Garante a data pelo banco também

        builder.Property(x => x.UpdatedAt)
            .IsRequired()
            .HasColumnType("DATETIME")
            .HasDefaultValueSql("GETDATE()");

        builder.Property(x => x.UserId)
            .IsRequired()
            .HasColumnType("VARCHAR")
            .HasMaxLength(160); // Tamanho seguro para IDs de Identity/Auth0

        // -------------------------------------------------------------
        // 4. Propriedade Calculada (IMPORTANTE)
        // -------------------------------------------------------------
        // O EF Core tentaria criar uma coluna "Total" se não avisarmos.
        // Como o cálculo é feito no C# (Get), devemos ignorar no banco.
        builder.Ignore(x => x.Total);

        // -------------------------------------------------------------
        // 5. Relacionamentos
        // -------------------------------------------------------------

        // 1 : N => Um Pedido tem Muitos Itens
        builder.HasMany(x => x.Items)
            .WithOne(x => x.Order)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade); 
            // Cascade: Se apagar o Pedido, apaga os Itens (cuidado com Hard Delete!)

        // N : 1 => Muitos Pedidos podem ter Um Voucher (ou nenhum)
        builder.HasOne(x => x.Voucher)
            .WithMany() // O Voucher não precisa ter uma lista de pedidos nele
            .HasForeignKey(x => x.VoucherId)
            .OnDelete(DeleteBehavior.SetNull); 
            // SetNull: Se o Voucher for apagado, o pedido continua existindo, 
            // mas o campo VoucherId vira null. Isso protege o histórico.

        // 6. Índices (Opcional mas recomendado)
        // Facilita buscar pedidos pelo "Número" (código) rapidamente
        builder.HasIndex(x => x.Number)
            .IsUnique();
    }
}
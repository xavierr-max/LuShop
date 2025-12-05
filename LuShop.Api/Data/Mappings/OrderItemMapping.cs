using LuShop.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LuShop.Api.Data.Mappings;

public class OrderItemMapping : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        // 1. Tabela
        builder.ToTable("OrderItems");

        // 2. Chave Primária
        builder.HasKey(x => x.Id);

        // 3. Propriedades
        builder.Property(x => x.Quantity)
            .IsRequired()
            .HasColumnType("INT");

        builder.Property(x => x.Price)
            .IsRequired()
            .HasColumnType("MONEY"); // Use MONEY ou DECIMAL(18,2) para valores monetários
            
        // 4. Relacionamentos

        // Relacionamento com ORDER (N:1)
        // Um Item pertence a UM Pedido
        builder.HasOne(x => x.Order)
            .WithMany(x => x.Items) // O pedido tem muitos itens
            .HasForeignKey(x => x.OrderId);
        // Obs: O OnDelete Cascade já foi configurado no OrderMapping, o EF entende.

        // Relacionamento com PRODUCT (N:1)
        // Um Item aponta para UM Produto
        builder.HasOne(x => x.Product)
            .WithMany() // O Produto NÃO precisa ter uma lista de OrderItems
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict); 
            
        /* IMPORTANTE: DeleteBehavior.Restrict
           Isso é o que garante o funcionamento do seu ProductHandler.DeleteAsync.
           Se você tentar dar um Hard Delete num produto que tem um OrderItem apontando para ele,
           o banco de dados vai BLOQUEAR e lançar a exceção (DbUpdateException) que capturamos lá.
        */
    }
}
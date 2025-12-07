using LuShop.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LuShop.Api.Data.Mappings;

public class CartItemMapping : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("CartItems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Quantity)
            .IsRequired()
            .HasDefaultValue(1);

        // Propriedade Calculada (NÃO CRIAR COLUNA NO BANCO)
        builder.Ignore(x => x.TotalPrice);

        // Relacionamento com Produto
        builder.HasOne(x => x.Product)
            .WithMany() // Produto não precisa listar em quais carrinhos está (seria custoso)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade); 
        // CUIDADO: Se deletar um Produto, ele some dos carrinhos dos clientes.
        // Em produção, geralmente usamos "Soft Delete" (IsActive = false) no Produto
        // para evitar que suma do carrinho, ou tratamos isso na aplicação.
    }
}
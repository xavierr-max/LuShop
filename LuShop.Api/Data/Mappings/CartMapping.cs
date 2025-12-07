using LuShop.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LuShop.Api.Data.Mappings;

public class CartMapping : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.ToTable("Carts");

        builder.HasKey(x => x.Id);

        // O UserId é crucial para buscar o carrinho. Criamos um índice para performance.
        builder.Property(x => x.UserId)
            .IsRequired()
            .HasMaxLength(160); // Ajuste conforme seu provedor de Identity (GUIDs usam menos, Emails usam mais)

        builder.HasIndex(x => x.UserId)
            .IsUnique(); // Garante que um usuário tenha apenas 1 carrinho ativo no banco

        // Propriedades Calculadas (NÃO CRIAR COLUNA NO BANCO)
        builder.Ignore(x => x.Total);
        builder.Ignore(x => x.SubTotal);

        // Relacionamento 1 : N (Carrinho tem muitos Itens)
        builder.HasMany(x => x.Items)
            .WithOne(x => x.Cart)
            .HasForeignKey(x => x.CartId)
            .OnDelete(DeleteBehavior.Cascade); // Se deletar o Carrinho, deleta os itens junto

        // Relacionamento Opcional com Voucher
        builder.HasOne(x => x.Voucher)
            .WithMany() // Voucher não precisa saber quais carrinhos o usam
            .HasForeignKey(x => x.VoucherId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull); // Se deletar o Voucher, o carrinho apenas fica sem desconto
    }
}
using LuShop.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LuShop.Api.Data.Mappings;

public class ProductMapping : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        // 1. Tabela
        builder.ToTable("Products");

        // 2. Chave Primária
        builder.HasKey(x => x.Id);

        // 3. Propriedades
        builder.Property(x => x.Title)
            .IsRequired()
            .HasColumnType("NVARCHAR") 
            .HasMaxLength(80);

        builder.Property(x => x.Description)
            .IsRequired()
            .HasColumnType("NVARCHAR")
            .HasMaxLength(255);

        builder.Property(x => x.Slug)
            .IsRequired()
            .HasColumnType("VARCHAR") 
            .HasMaxLength(80);

        // --- NOVO: Mapeamento da Imagem ---
        builder.Property(x => x.ImageUrl)
            .IsRequired(false) // É seguro deixar opcional (pode ter produtos antigos sem foto)
            .HasColumnType("VARCHAR") // O caminho é simples (ASCII), não precisa de acentos
            .HasMaxLength(255); // Espaço suficiente para URLs ou caminhos longos
        // ----------------------------------

        builder.Property(x => x.Price)
            .IsRequired()
            .HasColumnType("MONEY");

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasColumnType("BIT");

        // 4. Índices
        builder.HasIndex(x => x.Slug)
            .IsUnique();

        // 5. Relacionamentos
        builder.HasOne(x => x.Category)
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
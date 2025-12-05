using LuShop.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LuShop.Api.Data.Mappings;

public class CategoryMapping : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        // 1. Tabela
        builder.ToTable("Categories");

        // 2. Chave Primária
        builder.HasKey(x => x.Id);

        // 3. Propriedades
        builder.Property(x => x.Title)
            .IsRequired() // Obrigatório
            .HasColumnType("NVARCHAR") // Aceita acentos
            .HasMaxLength(80);

        builder.Property(x => x.Description)
            .IsRequired(false) // IMPORTANTE: Mapeia o 'string?' (Nullable)
            .HasColumnType("NVARCHAR")
            .HasMaxLength(255);

        // 4. Índices
        // Evita criar duas categorias com o mesmo nome (ex: duas categorias "Eletrônicos")
        builder.HasIndex(x => x.Title)
            .IsUnique();
            
        // Nota sobre Relacionamento:
        // O relacionamento (1:N) com Produtos já foi configurado no ProductMapping.
        // Como a classe Category não tem uma lista "public List<Product> Products",
        // não precisamos configurar nada extra aqui.
    }
}
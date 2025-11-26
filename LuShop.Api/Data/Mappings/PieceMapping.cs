using LuShop.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LuShop.Api.Data.Mappings;

public class PieceMapping : IEntityTypeConfiguration<Piece>
{
    public void Configure(EntityTypeBuilder<Piece> builder)
    {
        // 1. Mapeamento da Tabela
        builder.ToTable("Pieces");

        // 2. Chave Primária
        builder.HasKey(x => x.Id);
        
        // 3. Mapeamento de Propriedades

        // Id (A chave primária é tipicamente IDENTITY no SQL Server por padrão)
        builder.Property(x => x.Id)
               .ValueGeneratedOnAdd();
               
        // Title (Obrigatório, NVARCHAR com 150 caracteres)
        builder.Property(x => x.Title)
               .IsRequired()
               .HasColumnType("NVARCHAR")
               .HasMaxLength(150);

        // Description (Opcional, NVARCHAR com 500 caracteres)
        builder.Property(x => x.Description)
               .IsRequired(false) // Permite valores NULL
               .HasColumnType("NVARCHAR")
               .HasMaxLength(500);

        // Stock (Obrigatório, INT)
        builder.Property(x => x.Stock)
               .IsRequired()
               .HasColumnType("INT");
        
        // AvailabilityType (Enum como SMALLINT)
        builder.Property(x => x.Type)
               .IsRequired()
               .HasColumnType("SMALLINT"); // Mapeia o enum para um inteiro de 16 bits.

        // Price (Obrigatório, MONEY para valores monetários)
        builder.Property(x => x.Price)
               .IsRequired()
               .HasColumnType("MONEY");

        // UserId (Obrigatório, VARCHAR com 450 caracteres para Identity)
        builder.Property(x => x.UserId)
               .IsRequired()
               .HasColumnType("VARCHAR")
               .HasMaxLength(450); // Padrão comum para UserId do Identity.
    }
}
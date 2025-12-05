using LuShop.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LuShop.Api.Data.Mappings;

public class VoucherMapping : IEntityTypeConfiguration<Voucher>
{
    public void Configure(EntityTypeBuilder<Voucher> builder)
    {
        // 1. Tabela
        builder.ToTable("Vouchers");

        // 2. Chave Primária
        builder.HasKey(x => x.Id);

        // 3. Propriedades
        builder.Property(x => x.Number)
            .IsRequired()
            .HasColumnType("VARCHAR") // VARCHAR permite códigos flexíveis (ex: "PROMO2025")
            .HasMaxLength(80);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasColumnType("NVARCHAR") // Aceita acentos (ex: "Promoção de Verão")
            .HasMaxLength(80);

        builder.Property(x => x.Description)
            .IsRequired()
            .HasColumnType("NVARCHAR")
            .HasMaxLength(255);

        builder.Property(x => x.Amount)
            .IsRequired()
            .HasColumnType("MONEY"); // Segue o padrão monetário dos outros mapeamentos

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasColumnType("BIT");

        // 4. Índices
        // Regra de Ouro: Não podem existir dois vouchers com o mesmo código 'Number'
        builder.HasIndex(x => x.Number)
            .IsUnique();
            
        // Obs: O relacionamento com Order (1:N) já foi configurado lá no OrderMapping.
        // Como o Voucher não tem uma lista de Pedidos dentro dele, não precisamos configurar nada aqui.
    }
}
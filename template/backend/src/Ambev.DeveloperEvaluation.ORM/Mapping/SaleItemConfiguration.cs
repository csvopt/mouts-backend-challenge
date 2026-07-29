using Ambev.DeveloperEvaluation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambev.DeveloperEvaluation.ORM.Mapping;

public class SaleItemConfiguration : IEntityTypeConfiguration<SaleItem>
{
    public void Configure(EntityTypeBuilder<SaleItem> builder)
    {
        builder.ToTable("SaleItems");

        builder.HasKey(item => item.Id);
        builder.Property(item => item.Id).HasColumnType("uuid");
        builder.Property(item => item.SaleId).HasColumnType("uuid");
        builder.Property(item => item.ProductId).HasColumnType("uuid");

        builder.Property(item => item.ProductName)
            .IsRequired()
            .HasMaxLength(150);
        builder.Property(item => item.Quantity).IsRequired();
        builder.Property(item => item.UnitPrice)
            .IsRequired()
            .HasPrecision(18, 2);
        builder.Property(item => item.DiscountPercentage)
            .IsRequired()
            .HasPrecision(5, 4);
        builder.Property(item => item.TotalAmount)
            .IsRequired()
            .HasPrecision(18, 2);
        builder.Property(item => item.IsCancelled).IsRequired();
        builder.Property(item => item.CancelledAt)
            .HasColumnType("timestamp with time zone");

        builder.HasIndex(item => new { item.SaleId, item.ProductId }).IsUnique();
    }
}

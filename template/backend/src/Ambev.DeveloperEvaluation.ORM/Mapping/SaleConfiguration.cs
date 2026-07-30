using Ambev.DeveloperEvaluation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambev.DeveloperEvaluation.ORM.Mapping;

public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("Sales");

        builder.HasKey(sale => sale.Id);
        builder.Property(sale => sale.Id).HasColumnType("uuid");

        builder.Property(sale => sale.SaleNumber)
            .IsRequired()
            .HasMaxLength(50);
        builder.HasIndex(sale => sale.SaleNumber).IsUnique();

        builder.Property(sale => sale.Date)
            .IsRequired()
            .HasColumnType("timestamp with time zone");

        builder.Property(sale => sale.CustomerId)
            .IsRequired()
            .HasColumnType("uuid");
        builder.Property(sale => sale.CustomerName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(sale => sale.BranchId)
            .IsRequired()
            .HasColumnType("uuid");
        builder.Property(sale => sale.BranchName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(sale => sale.TotalAmount)
            .IsRequired()
            .HasPrecision(18, 2);
        builder.Property(sale => sale.IsCancelled).IsRequired();
        builder.Property(sale => sale.CancelledAt)
            .HasColumnType("timestamp with time zone");

        builder.HasMany(sale => sale.Items)
            .WithOne(item => item.Sale)
            .HasForeignKey(item => item.SaleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(sale => sale.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class SubastaConfiguration : IEntityTypeConfiguration<Subasta>
{
    public void Configure(EntityTypeBuilder<Subasta> builder)
    {
        builder.ToTable("Subastas");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Titulo)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.PrecioBase)
            .HasPrecision(12, 2);

        builder.Property(x => x.IncrementoMinimo)
            .HasPrecision(12, 2);

        builder.Property(x => x.Estado)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(x => x.FechaInicio)
            .IsRequired();

        builder.Property(x => x.FechaFin)
            .IsRequired();

        builder.Property(x => x.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(x => x.VendedorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(x => x.GanadorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
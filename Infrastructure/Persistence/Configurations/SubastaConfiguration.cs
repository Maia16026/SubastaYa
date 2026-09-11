using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

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

        builder.Property(x => x.PrecioBase);

        builder.Property(x => x.IncrementoMinimo);

        builder.Property(x => x.Estado)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
            v => v,
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        builder.Property(x => x.FechaInicio)
            .IsRequired()
            .HasConversion(dateTimeConverter);

        builder.Property(x => x.FechaFin)
            .IsRequired()
            .HasConversion(dateTimeConverter);
        // Version como concurrency token (optimistic locking)
        builder.Property(x => x.Version)
            .IsConcurrencyToken();

        // Relación Vendedor (Usuario) 1:N explícita
        builder.HasOne(s => s.Vendedor)
            .WithMany(u => u.Subastas)
            .HasForeignKey(s => s.VendedorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relación Categoria 1:N
        builder.HasOne(s => s.Categoria)
            .WithMany(c => c.Subastas)
            .HasForeignKey(s => s.CategoriaId);
    }
}
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class MovimientoConfiguration : IEntityTypeConfiguration<Movimiento>
{
    public void Configure(EntityTypeBuilder<Movimiento> builder)
    {
        builder.ToTable("Movimientos");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Tipo)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(x => x.Monto)
            .IsRequired()
            .HasPrecision(12, 2);

        builder.Property(x => x.FechaHora)
            .IsRequired();

        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(x => x.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new
        {
            x.UsuarioId,
            x.FechaHora
        });
    }
}
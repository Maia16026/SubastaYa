using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class BilleteraConfiguration : IEntityTypeConfiguration<Billetera>
{
    public void Configure(EntityTypeBuilder<Billetera> builder)
    {
        builder.ToTable("Billeteras");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UsuarioId)
            .IsRequired();

        builder.Property(x => x.SaldoTotal);

        builder.Property(x => x.SaldoRetenido);

        builder.Property(x => x.SaldoDisponible);

        builder.Property(x => x.Version)
            .IsConcurrencyToken();

        // Relación 1:1 Billetera -> Usuario (FK en Billetera.UsuarioId)
        builder.HasOne(b => b.Usuario)
            .WithOne(u => u.Billetera)
            .HasForeignKey<Billetera>(b => b.UsuarioId);
    }
}
